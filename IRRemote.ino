#include <IRremote.h>

int RECV_PIN = 11;

IRrecv irrecv(RECV_PIN);

decode_results results;

int lastkey;

void setup()
{
  Serial.begin(115200);
  irrecv.enableIRIn(); // Start the receiver
}

void loop() {
  if (irrecv.decode(&results)) {
    keystroke();
    delay(80);
    irrecv.resume(); // Receive the next value
  }
}

void keystroke() {
  int key; // Getted key
  double received = results.value; // Received key

  // Check repeater code
  if (received == 4294967295) {
    // It's repeating
    key = lastkey;
  }
  else {
    // It's not repeating
    key = received;
    lastkey = received;
  }

  Serial.println(key, HEX);
}
