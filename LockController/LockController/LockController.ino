#include <ESP8266WiFi.h>
#include <ESP8266WebServer.h>


/*Put your SSID & Password*/
const char* ssid = "NodeMCU_Station_Mode";  // Enter SSID here
const char* password = "h5pd091121_Styrer";  //Enter Password here

// GET: 192.168.10.186/togglelock

ESP8266WebServer server(80);

uint8_t LED1 = D7;
bool LED1status = LOW;

// first part is the id of this controller. Used to specify which controller to send request to
// Second part is the id of the API that is allowed to send requests to this controller
const char* connStr = "/testlock/testapi";

void setup() {
    Serial.begin(115200);
    delay(100);

    Serial.println("Connecting to ");
    Serial.println(ssid);

    //connect to your local wi-fi network
    WiFi.begin(ssid, password);

    //check wi-fi is connected to wi-fi network
    while (WiFi.status() != WL_CONNECTED) {
        delay(1000);
        Serial.print(".");
    }
    Serial.println("");



    Serial.println("");
    Serial.println("WiFi connected..!");
    Serial.print("Got IP: ");  Serial.println(WiFi.localIP());

    server.on(connStr, toggleLock);

    server.begin();
    Serial.println("HTTP server started");

    pinMode(LED1, OUTPUT);
    digitalWrite(LED1, LED1status);
}

void toggleLock() {
    LED1status = !LED1status;
    server.send(200);
    digitalWrite(LED1, LED1status);   // turn the LED on and off
}

void loop() {
    server.handleClient();
}
