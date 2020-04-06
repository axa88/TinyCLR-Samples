// yahboom
// https://github.com/lzty634158/Tiny-bit
// P0 buzzer
// P1 voice sensor/NC.... is there a jumper? J3?
// P12 neopixel x2
// P13 left lines sensor, digital
// P14 right line sensor, digital
// P15 ultrasonic echo
// P15 ultrasonic trig


// P19 I2C for PWM address 0x01. unknown chip!
// P20 I2C for PWM
// for RGB 0x01, red, green, blue
// for motor 0x02, left, left, right, right

using System;
using System.Threading;
using GHIElectronics.TinyCLR.Devices.I2c;
using GHIElectronics.TinyCLR.Devices.Adc;
using GHIElectronics.TinyCLR.Devices.Gpio;
using GHIElectronics.TinyCLR.Drivers.Neopixel.WS2812;
using GHIElectronics.TinyCLR.Devices.Pwm;

namespace GHIElectronics.TinyCLR.Yahboom.TinyBit {
    class TinyBitController {
        private I2cDevice i2c;
        private AdcChannel voiceSensor;
        private GpioPin leftLineSensor, rightLineSensor;
        private WS2812 ws2812;
        private byte[] b4 = new byte[4];
        private byte[] b5 = new byte[5];
        private PwmChannel buzzer;
        public void Beep() {
            this.buzzer.Controller.SetDesiredFrequency(4000);
            this.buzzer.SetActiveDutyCyclePercentage(0.5);
            this.buzzer.Start();
            Thread.Sleep(50);
            this.buzzer.Stop();
        }
        public void SetMotorSpeed(double left, double right) {
            this.b5[0] = 0x02;

            if (left > 0) {
                this.b5[1] = (byte)(left * 255);
                this.b5[2] = 0x00;
            }
            else {
                left *= -1;
                this.b5[1] = 0x00;
                this.b5[2] = (byte)(left * 255);
            }

            if (right > 0) {
                this.b5[3] = (byte)(right * 255);
                this.b5[4] = 0x00;
            }
            else {
                right *= -1;
                this.b5[3] = 0x00;
                this.b5[4] = (byte)(right * 255);
            }
            this.i2c.Write(this.b5);
        }
        public void SetHeadlight(int red, int green, int blue) {
            this.b4[0] = 0x01;
            this.b4[1] = (byte)(red);
            this.b4[2] = (byte)(green);
            this.b4[3] = (byte)(blue);
            this.i2c.Write(this.b4);
        }
        public TinyBitController(I2cController i2cController, PwmChannel buzzer, AdcChannel voiceSensor, GpioPin leftLineSensor, GpioPin rightLineSensor, int colorLedPin) {
            this.i2c = i2cController.GetDevice(new I2cConnectionSettings(0x01,400_000));
            this.buzzer = buzzer;
            this.voiceSensor = voiceSensor;
            this.leftLineSensor = leftLineSensor;
            this.leftLineSensor.SetDriveMode(GpioPinDriveMode.Input);
            this.rightLineSensor = rightLineSensor;
            this.rightLineSensor.SetDriveMode(GpioPinDriveMode.Input);
            this.ws2812 = new WS2812(colorLedPin, 2);

        }
        public bool ReadLineSensor(bool left) {
            if (left)
                return this.leftLineSensor.Read() == GpioPinValue.High;
            else
                return this.rightLineSensor.Read() == GpioPinValue.High;
        }
        public double ReadVoiceLevel() => this.voiceSensor.ReadRatio();
        public void SetColorLeds(int index, int red, int green, int blue) {
            this.ws2812.SetColor(index, red, green, blue);
            this.ws2812.Draw();
        }
    }
}
