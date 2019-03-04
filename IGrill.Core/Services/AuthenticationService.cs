using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;

namespace IGrill.Core
{
    class AuthenticationService
    {
        private Random rnd = new Random();

        public static readonly Guid DEVICE_SERVICE_GUID = Guid.Parse("64AC0000-4A4B-4B58-9F37-94D3C52FFDF7");

        public static readonly Guid APP_CHALLENGE_GUID = Guid.Parse("64AC0002-4A4B-4B58-9F37-94D3C52FFDF7");
        public static readonly Guid DEVICE_CHALLENGE_GUID = Guid.Parse("64AC0003-4A4B-4B58-9F37-94D3C52FFDF7");
        public static readonly Guid DEVICE_RESPONSE_GUID = Guid.Parse("64AC0004-4A4B-4B58-9F37-94D3C52FFDF7");

        private readonly IGrillVersion iGrillVersion;

        // iGrill Mini 
        private readonly byte[] iGrillMiniKey = new byte[] {
            0xED, 0x5E, 0x30, 0x8E, 0x8B, 0xCC, 0x91, 0x13,
            0x30, 0x6C, 0xD4, 0x68, 0x54, 0x15, 0x3E, 0xDD  };

        // iGrill v2 & iGrill Mini 2
        private readonly byte[] iGrill2Key = new byte[] {
            0xDF, 0x33, 0xE0, 0x89, 0xF4, 0x48, 0x4E, 0x73,
            0x92, 0xD4, 0xCF, 0xB9, 0x46, 0xE7, 0x85, 0xB6  };

        // iGrill v3 ?? correct?
        private readonly byte[] iGrill3Key = new byte[]
        {
            0x27, 0x62, 0xFC, 0x5E, 0xCA, 0x13, 0x45, 0xE5,
            0x9D, 0x11, 0xDE, 0xB6, 0xF6, 0xF3, 0x8C, 0x1C
        };
        /*
        return new byte[]{81, 16, 95, 92, 7, 107, 33, -14, 50, 31, -86, -28, 104, -21, -85, -25}; pulse 1000
        return new byte[]{39, 98, -4, 94, -54, 19, 69, -27, -99, 17, -34, 74, -10, -13, -116, 28}; // kt ? iGrill v3
        return new byte[]{8, 105, 3, 62, -29, -80, 68, -16, -108, 127, 67, -114, -61, -97, 80, 88};  // igrill_v3
        return new byte[]{42, -31, -84, 99, -121, -73, 65, -83, -81, -111, 53, 75, -59, 6, -62, -70}; // igrill_v2_2
        */


        public AuthenticationService(IGrillVersion iGrillVersion)
        {
            this.iGrillVersion = iGrillVersion;
        }

        public async Task Authenticate(BluetoothLEDevice bluetoothLeDevice)
        {
            Debug.WriteLine("Start authentication with iGrill");

            // Gett Service
            var service = await bluetoothLeDevice.GetGattServiceForUuidAsync(DEVICE_SERVICE_GUID);

            var characteristics = await service.GetCharacteristics2Async();
            var challengeCharacteristic = characteristics.First((c) => c.Uuid == APP_CHALLENGE_GUID);
            var deviceChallengeCharacteristig = characteristics.First((c) => c.Uuid == DEVICE_CHALLENGE_GUID);
            var responseCharacterisitc = characteristics.First((c) => c.Uuid == DEVICE_RESPONSE_GUID);

            var encryptionKey = GetEncryptionKey(iGrillVersion);

            Debug.WriteLine("Send challenge to iGrill");
            var challenge = new byte[16];
            Array.Copy(Enumerable.Range(0, 8).Select(n => (byte)rnd.Next(0, 255)).ToArray(), challenge, 8);
            await challengeCharacteristic.WriteBytesAsync(challenge);

            // read device challenge
            Debug.WriteLine("Read encrypted challenge from iGrill");
            byte[] encrypted_device_challenge = await deviceChallengeCharacteristig.ReadBytesAsync();
            var device_challenge = Encryption.Decrypt(encrypted_device_challenge, encryptionKey);

            // verify device challenge
            Debug.WriteLine("Comparing challenges...");
            Enumerable.Range(0, 8).ToList().ForEach(i => {
                if (challenge[i] != device_challenge[i])
                    throw new Exception("Invalid device challange");
            });

            // send device response
            Debug.WriteLine("Send encrypted response to iGrill");
            var device_response = new byte[16];
            Array.Copy(device_challenge, 8, device_response, 8, 8);

            var encrypted_device_response = Encryption.Encrypt(device_response, encryptionKey);
            await responseCharacterisitc.WriteBytesAsync(encrypted_device_response);

            // Authenticated
            Debug.WriteLine("Authentication with iGrill successful");
        }

        private byte[] GetEncryptionKey(IGrillVersion iGrillVersion)
        {
            switch (iGrillVersion)
            {
                case IGrillVersion.IGrillMini:
                    return iGrillMiniKey;
                case IGrillVersion.IGrill2:
                    return iGrill2Key;
                case IGrillVersion.IGrill3:
                    return iGrill3Key;
                default:
                    throw new Exception(String.Format("No key configured for iGrill Version {0}", iGrillVersion));
            }
        }
    }
}
