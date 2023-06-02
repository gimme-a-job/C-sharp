import sys
sys.path.append(r"C:\Program Files\Python311\Lib")

import base64
import json
import jwt

import requests

# "https://docs.aws.amazon.com/ja_jp/elasticloadbalancing/latest/application/listener-authenticate-users.html"より
# def getPayload(headers, region="ap-northeast-1"): # 元
def getPayload(x_amzn_oidc_data, region="ap-northeast-1"): # header型に出来ないので妥協
    
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
    pub_key = req.text # 現状これは使えない
    pub_key = "-----BEGIN PUBLIC KEY-----\nMFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEEVs/o5+uQbTjL3chynL4wXgUg2R9\nq9UU8I5mEovUf86QZ7kOBIjJwqnzD1omageEHWwHdBO6B+dFabmdT9POxg==\n-----END PUBLIC KEY-----\n"

    # Step 3: Get the payload
    payload = jwt.decode(encoded_jwt, pub_key, algorithms=['ES256'])
    return payload

#x_amzn_oidc_data = str(sys.argv[1])

## x_amzn_oidc_data = str(sys.arg_str[0])

#print(getPayload(x_amzn_oidc_data))

## 