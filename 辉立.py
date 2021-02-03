#pip install pyotp
#pip install requests
import requests
import pyotp

#申购基础信息
val_code = str(input("申购新股编号："))
val_price = str(input("请输入单股价格："))
val_count = str(input("请输入申购股数："))
val_choose = str(input("输入申购类型，38餐输入1，9成孖展请输入2，95成输入3"))
val_user = 'M324xxx'
val_pass = 'Dhxxxxxx'
#post条件中BQty中第二部分金额生成
val_pay = round(val1*val2+val1*val2*0.01+val1*val2*0.000027+val1*val2*0.00005,2)
#post条件中BQty生成
valBQty =  "\'"+val_count+","+val_pay+"\'"
#token自动生成
#key需要解绑，然后从网页版登录获取对应的绑定码，去掉第五位和第十七位，输入val_key
val_key = 'XXXXXXXXXXXXXXXXXXXX'
totp = pyotp.TOTP(val_key)
token = totp.now()


#一次登录
url = 'https://trading.poems.com.hk/Poems2/LoginAction.asp'
data = {'func': 'Login', 'Language': 'ZH', 'IPO': '','iFormType': '','Accode': val_user,'Password': val_pass}
r = requests.post(url,data=data,verify=False)
print(r.text)
cookie = requests.utils.dict_from_cookiejar(r.cookies)
#print(cookie)

#二次登录，验证码待生成
url = 'https://trading.poems.com.hk/Poems2/includeFolder/loginOTP.asp'
data = {'iFunc': 'LOGIN', 'Language': '', 'IPO': '', 'pErrMsg': '', 'iFormType': '', 'pLoginId': val_user,'iOTP': token}

r = requests.post(url,data=data,cookies=cookie,verify=False)
#print(r.text)


#申购部分
url = "https://trading.poems.com.hk/Poems2/ProductPlatform/LStock/IPO_New/InputIPOAction.asp"

#10成，side为3
data = {'func': 'Insert', 'WebSite': '', 'Popup': 'N', 'SCTYCode': val_code, 'OrderPrice': val_price, 'adminFee': '0','OrderQty': val_count, 'BQty': valBQty, 'OrderSide': '3', 'ExchangeProduct_Point': '1500','ExchangeProduct_Price': '100'}
r = requests.post(url,data=data,verify=False)

##9成 ，side为7
data = {'func': 'Insert', 'WebSite': '', 'Popup': 'N', 'SCTYCode': val_code, 'OrderPrice': val_price, 'adminFee': '0','OrderQty': val_count, 'BQty': valBQty, 'OrderSide': '7', 'ExchangeProduct_Point': '1500','ExchangeProduct_Price': '100'}  

##9.5成，side为5

data = {'func': 'Insert', 'WebSite': '', 'Popup': 'N', 'SCTYCode': val_code, 'OrderPrice': val_price, 'adminFee': '0','OrderQty': val_count, 'BQty': valBQty, 'OrderSide': '3', 'ExchangeProduct_Point': '1500','ExchangeProduct_Price': '100'}
