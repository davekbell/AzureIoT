from base64 import b64encode, b64decode
from hashlib import sha256
import time
from urllib import quote_plus, urlencode
from hmac import HMAC
import paho.mqtt.client as paho
import random

# Function to generate a SAS token which we use later as part of the MQTT password"
def generate_sas_token(uri, key, policy_name, expiry=86400):
    ttl = time.time() + expiry
    sign_key = "%s\n%d" % ((quote_plus(uri)), int(ttl))
    print("Signing Key: \n" + sign_key)
    signature = b64encode(HMAC(b64decode(key), sign_key, sha256).digest())

    rawtoken = {
        'sr' :  uri,
        'sig': signature,
        'se' : str(int(ttl))
    }

    if policy_name is not None:
        rawtoken['skn'] = policy_name

    return 'SharedAccessSignature ' + urlencode(rawtoken)

# When MQTT client connects, this function is called
def on_connect(client, userdata, flags, rc):
    print("CONNACK received with code % d.\n" % (rc))

# When MQTT client publishes, this function is called
def on_publish(client, userdata, mid):
    print("Data: " + temperature0 + " " + "Result code: " + str(rc) + " " + "Message ID: " + str(mid))

# Call the generate_sas_token function and assign the result to SAStoken variable
SAStoken = generate_sas_token(uri="etisalat-iot-hub-etl.azure-devices.net%2Fdevices%2Fabbtestdevice", key="+i5/3u0DsYoZc0FCKVD2iReK6XFG65ET2/la5MC2PHA=", policy_name=None)


print("\nGenerated SAS Token: " + SAStoken)

# Set parameters for the MQTT client
mqtt_username = "etisalat-iot-hub-etl.azure-devices.net/abbtestdevice/api-version=2018-06-30"
mqtt_password = SAStoken
mqtt_client_id = "abbtestdevice"
mqtt_broker = "13.95.15.251"


# Create MQTT client
mqtt_client = paho.Client(client_id=mqtt_client_id, clean_session=False, userdata=None, protocol=paho.MQTTv311)

# Define which functions are called by MQTT client upon certain actions
mqtt_client.on_connect = on_connect
mqtt_client.on_publish = on_publish

# Set MQTT client username and password
mqtt_client.username_pw_set(mqtt_username, mqtt_password)

print("\nConnecting...\n")

# Tell MQTT client to connect using TLS encryption with self-signed certificates
mqtt_client.tls_set()
mqtt_client.tls_insecure_set(True)

# Connect MQTT client to IoT Hub
mqtt_client.connect(host=mqtt_broker, port=8883, keepalive=240, bind_address="")

# Start the MQTT client thread
mqtt_client.loop_start()

while True:
    temperature0 = "{\"Label\":" + str(random.randint(0, 23)) + \
                    ",\"House\":" + str(random.randint(0, 500)) + \
                    ",\"Yr\":" + str(random.randint(2011, 2013)) + \
                    ",\"Mon\":" + str(random.randint(1, 12)) + \
                    ",\"Temperature\":" + format(random.normalvariate(25, 5), ".2f") + \
                    ",\"Daylight\":" + str(random.randint(400, 1000)) + \
                    "}"

    (rc, mid) = mqtt_client.publish("devices/abbtestdevice/messages/events/", temperature0, qos=0, retain=True)
    time.sleep(2)
