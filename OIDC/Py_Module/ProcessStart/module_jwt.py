import base64
import json
import jwt

import sys

import requests

from enum import Enum

STATUS = 'response_status'
EX = 'exception'
PAYLOAD = 'payload'

class ResponseStatus(Enum):
    NG = 0
    OK = 1

# "https://docs.aws.amazon.com/ja_jp/elasticloadbalancing/latest/application/listener-authenticate-users.html"より
# def getPayload(headers, region="ap-northeast-1"): # 元
def getPayload(x_amzn_oidc_data: str, region="ap-northeast-1"): # .NETからトークンを取得

    # Step 1: Get the key id from JWT headers (the kid field)
    #  encoded_jwt = headers.dict['x-amzn-oidc-data'] # 元
    encoded_jwt = str(x_amzn_oidc_data)
    jwt_headers = encoded_jwt.split('.')[0]
    decoded_jwt_headers = base64.b64decode(jwt_headers)
    decoded_jwt_headers = decoded_jwt_headers.decode("utf-8")
    decoded_json = json.loads(decoded_jwt_headers)
    kid = decoded_json['kid']

    # Step 2: Get the public key from regional endpoint
    url = 'https://public-keys.auth.elb.' + str(region) + '.amazonaws.com/' + str(kid)
    req = requests.get(url)
    pub_key = req.text # 本番環境以外(つまりAWS無し)ではこれは使えない
    pub_key = "-----BEGIN PUBLIC KEY-----\n" \
        + "MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEEVs/o5+uQbTjL3chynL4wXgUg2R9\n" \
            + "q9UU8I5mEovUf86QZ7kOBIjJwqnzD1omageEHWwHdBO6B+dFabmdT9POxg==\n" \
                + "-----END PUBLIC KEY-----\n" # (AWS無し環境の)テスト用公開鍵

    # Step 3: Get the payload
    payload = jwt.decode(encoded_jwt, pub_key, algorithms=['ES256'])
    return payload

# 例外の内容をセット
def setException(res: dict, ex: Exception):
    res[STATUS] = ResponseStatus.NG.value
    res[EX] =  ex.args[0]

# 辞書型をJSONにして.NETに渡す
def returnJson(res: dict):
    print(json.dumps(res))

# メイン処理
res = {}

try:
    # 本番用
    x_amzn_oidc_data = str(sys.argv[1])
    # テスト用
    # x_amzn_oidc_data = '自分で作るかAWSから引っ張ってきてください'
    
    res[PAYLOAD] = getPayload(x_amzn_oidc_data)     

except Exception as ex:    
    setException(res, ex)

else:
    res[STATUS] = ResponseStatus.OK.value
finally:
    
    try:
        returnJson(res)
        
    # 辞書型resのjson化に失敗した場合
    except Exception as jsonEx:
        res = {} # これをせず別変数を宣言しても良い
        setException(res, jsonEx)
        returnJson(res)
