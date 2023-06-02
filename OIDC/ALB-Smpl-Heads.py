import binascii
import jwt
import requests
import base64
import json
import urllib3

from jwt.algorithms import (
    Algorithm,
    get_default_algorithms,
    has_crypto,
    requires_cryptography,
)

from jwt.utils import base64url_decode, base64url_encode

from enum import Enum

# "https://docs.aws.amazon.com/ja_jp/elasticloadbalancing/latest/application/listener-authenticate-users.html"より
# def getDecoded_OIDCData_Payload(headers, region="ap-northeast-1"): # 元の値
def getDecoded_OIDCData_Payload(headers_dict, region="ap-northeast-1"): # header型に出来ないので妥協

 # Step 1: Get the key id from JWT headers (the kid field)
#  encoded_jwt = headers.dict['x-amzn-oidc-data'] # 元の値
 encoded_jwt = headers_dict['x-amzn-oidc-data'] # header型に出来ないので妥協
 jwt_headers = encoded_jwt.split('.')[0]
 decoded_jwt_headers = base64.b64decode(jwt_headers)
 decoded_jwt_headers = decoded_jwt_headers.decode("utf-8")
 decoded_json = json.loads(decoded_jwt_headers)
 kid = decoded_json['kid']

 # Step 2: Get the public key from regional endpoint
 url = 'https://public-keys.auth.elb.' + region + '.amazonaws.com/' + kid
 req = requests.get(url)
 pub_key = req.text # 現状これは使えない
 pub_key = "-----BEGIN PUBLIC KEY-----\nMFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEEVs/o5+uQbTjL3chynL4wXgUg2R9\nq9UU8I5mEovUf86QZ7kOBIjJwqnzD1omageEHWwHdBO6B+dFabmdT9POxg==\n-----END PUBLIC KEY-----\n"

 # Step 3: Get the payload
 payload = jwt.decode(encoded_jwt, pub_key, algorithms=['ES256'])
 return payload


# 列挙型の定義
# "https://atmarkit.itmedia.co.jp/ait/articles/2206/14/news020.html"
class Base64Mode(Enum):
    BASE64URL = 1
    BINASCII = 2

class Padding(Enum):
    OFF = 0
    ON = 1
    
def base64_encode(bytes, mode=Base64Mode.BASE64URL, Padding=Padding.ON):
    # パターン１ #こちらはpadding(する場合)'='を使う
    if mode == Base64Mode.BASE64URL:
        bytes = base64url_encode(bytes)
        # if Padding == Padding.ON: bytes = bytes + b'=' * (len(bytes) % 4) #こちらはpaddingとして'='を使う ~2023.05.19
        if Padding == Padding.ON: bytes = bytes + b'=' * (4 - (len(bytes) % 4)) #こちらはpaddingとして'='を使う 2023.05.19~
        return bytes
    # パターン２ #こちらはpaddingとして改行文字が入る
    elif mode == Base64Mode.BINASCII:
        bytes = binascii.b2a_base64(bytes) #こちらはpaddingとして改行文字が入る
        if Padding == Padding.ON: bytes = bytes + b'=' * (4 - (len(bytes) % 4)) #こちらはpaddingとして'='を使う 2023.05.19~
        return bytes
    
    return bytes
        

# 参考にしたjwtライブラリ
# "https://github.com/jpadilla/pyjwt/tree/master/jwt"
# "https://github.com/jpadilla/pyjwt/blob/master/jwt/api_jwt.py"
# "https://github.com/jpadilla/pyjwt/blob/master/jwt/api_jws.py"
def encode(oidc_data_headers, prv_key, oidc_data_payloads):
    # 完成版？
    segments = []
    
    encoded_jwt_headers = json.dumps(oidc_data_headers)
    encoded_jwt_headers = encoded_jwt_headers.encode("utf-8")
    # パターン１ #こちらはpaddingとして'='を使う
    # encoded_jwt_headers = base64_encode(encoded_jwt_headers, Base64Mode.BASE64URL, Padding.OFF) # binascii.Error: Incorrect padding を吐いてしまう
    encoded_jwt_headers = base64_encode(encoded_jwt_headers, Base64Mode.BASE64URL, Padding.ON) # 現状ヘッダはPaddingしないと動かないため
    # パターン２ #こちらはpaddingとして改行文字が入る
    # encoded_jwt_headers = base64_encode(encoded_jwt_headers, Base64Mode.BINASCII)
    # encoded_jwt_headers = base64_encode(encoded_jwt_headers, Base64Mode.BINASCII, Padding.ON)
    
    segments.append(encoded_jwt_headers)
    
    encoded_jwt_payloads = json.dumps(oidc_data_payloads)
    encoded_jwt_payloads = encoded_jwt_payloads.encode("utf-8")
    # パターン１ #こちらはpadding(する場合)'='を使う
    # encoded_jwt_payloads = base64_encode(encoded_jwt_payloads, Base64Mode.BASE64URL, Padding.OFF) # 暫定標準、こちらでも動く
    encoded_jwt_payloads = base64_encode(encoded_jwt_payloads, Base64Mode.BASE64URL, Padding.ON)
    # パターン２ #こちらはpaddingとして改行文字が入る
    # encoded_jwt_payloads = base64_encode(encoded_jwt_payloads, Base64Mode.BINASCII)
    # encoded_jwt_payloads = base64_encode(encoded_jwt_payloads, Base64Mode.BINASCII, Padding.ON)
    
    segments.append(encoded_jwt_payloads)
    
    # Segments
    signing_input = b".".join(segments)
    
    alg_obj = jwt.get_algorithm_by_name('ES256')
    key = alg_obj.prepare_key(prv_key)
    signature = alg_obj.sign(signing_input, key)
    
    # パターン１ #こちらはpadding(する場合)'='を使う
    # segments.append(base64_encode(signature, Base64Mode.BASE64URL, Padding.OFF)) # 暫定標準、こちらでも動く
    segments.append(base64_encode(signature, Base64Mode.BASE64URL, Padding.ON))
    # パターン２ #こちらはpaddingとして改行文字が入る
    # segments.append(base64_encode(signature, Base64Mode.BINASCII))
    # segments.append(base64_encode(signature, Base64Mode.BINASCII, Padding.ON))
    
    encoded_string = b".".join(segments)
    
    encoded_jwt = encoded_string.decode("utf-8")
    
    return encoded_jwt
    

def getEncodedHeaders(
    oidc_data_headers={
        "typ" : "JWT",
        "kid" : "{kid}",
        "alg" : "ES256",
        "iss" : "https://login.microsoftonline.com/{}}/v2.0",
        "client" : "{client}",
        "signer" : "{signer}}",
        "exp" : 1681782273 # 元の値
        # "exp" : 2681782273
        }, 
    prv_key="-----BEGIN PRIVATE KEY-----\nMIGHAgEAMBMGByqGSM49AgEGCCqGSM49AwEHBG0wawIBAQQgevZzL1gdAFr88hb2\nOF/2NxApJCzGCEDdfSp6VQO30hyhRANCAAQRWz+jn65BtOMvdyHKcvjBeBSDZH2r\n1RTwjmYSi9R/zpBnuQ4EiMnCqfMPWiZqB4QdbAd0E7oH50VpuZ1P087G\n-----END PRIVATE KEY-----\n",
    oidc_data_payloads={
        "sub" : "{sub}}",
        "name" : "テスト ユーザー",
        "picture" : "https://graph.microsoft.com/v1.0/me/photo/$value",
        "email" : "test.user@test.jp",
        "exp" : 1681782273, # 元の値
        # "exp" : 2681782273,
        "iss" : "https://login.microsoftonline.com/{iss}/v2.0"
    }):
    
    encoded_jwt = encode(oidc_data_headers, prv_key, oidc_data_payloads)
    
    headers_dict = {
        "x-amzn-oidc-data" : encoded_jwt
    }
    
    # requests.response.header型に変換したいがよく分からない
    
    return headers_dict
    # return headers_dict['x-amzn-oidc-data']


# 使ってない
def STEP_2(region="ap-northeast-1", kid="{kid}"):
    # Step 2: Get the public key from regional endpoint
    url = 'https://public-keys.auth.elb.' + region + '.amazonaws.com/' + kid
    req = requests.get(url)
    pub_key = req.text
    
    return pub_key


# for i in getEncoded_OIDCData():
#     for j in i:
#         for k in j:
#             print(k)

headers_dict = getEncodedHeaders()
oidc_data = "x-amzn-oidc-data"
print(oidc_data + ": " + headers_dict[oidc_data])
print(getDecoded_OIDCData_Payload(headers_dict))