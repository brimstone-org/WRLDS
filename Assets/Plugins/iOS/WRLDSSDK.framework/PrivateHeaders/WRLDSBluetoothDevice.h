//
//  WRLDSBluetoothDevice.h
//  WRLDSSDK
//
//  Created by Patrik Nyblad on 2018-07-05.
//

#import <Foundation/Foundation.h>
#import <CoreBluetooth/CoreBluetooth.h>

@class WRLDSBluetoothDevice;

/**
 This protocol is used by the WRLDSBluetoothScanner to send updates regarding advertisement info and signal strength.
 */
@protocol WRLDSBluetoothDeviceScanUpdateProtocol <NSObject>
@required

- (void)didUpdateRssi:(NSNumber *)rssi;
- (void)didUpdateAdvertisementInfo:(NSDictionary<NSString *,id> *)advertisementInfo;
- (void)didConnect;
- (void)didDisconnect:(NSError *)error;

@end

@protocol WRLDSBluetoothDeviceDelegate <NSObject>
@optional

- (void)bluetoothDeviceDidConnect:(WRLDSBluetoothDevice *)bluetoothDevice;
- (void)bluetoothDevice:(WRLDSBluetoothDevice *)bluetoothDevice didDisconnectWithError:(NSError *)error;
- (void)bluetoothDevice:(WRLDSBluetoothDevice *)bluetoothDevice didReadRssi:(NSNumber *)rssi;
- (void)bluetoothDevice:(WRLDSBluetoothDevice *)bluetoothDevice didReceiveAdvertisingInfo:(NSDictionary *)advertisingInfo;
- (void)bluetoothDevice:(WRLDSBluetoothDevice *)bluetoothDevice didDiscoverServices:(NSArray<CBService *> *)services;
- (void)bluetoothDevice:(WRLDSBluetoothDevice *)bluetoothDevice didDiscoverCharacteristicsForService:(CBService *)service;
- (void)bluetoothDevice:(WRLDSBluetoothDevice *)bluetoothDevice didUpdateValueForCharacteristic:(CBCharacteristic *)characteristic error:(NSError *)error;

@end


@interface WRLDSBluetoothDevice : NSObject <WRLDSBluetoothDeviceScanUpdateProtocol>

@property (nonatomic, weak) id<WRLDSBluetoothDeviceDelegate> delegate;
@property (nonatomic, strong, readonly) NSUUID *identifier;
@property (nonatomic, strong, readonly) NSNumber *rssi;
@property (nonatomic, strong, readonly) NSString *name;
@property (nonatomic, strong, readonly) NSDictionary *advertisingInfo;
/**
 A list of CBService objects that have been discovered on the peripheral.
 */
@property (nonatomic, strong, readonly) NSArray<CBService *> *services;

@property (nonatomic, strong, readonly) CBPeripheral *peripheral;

+ (instancetype)deviceWithPeripheral:(CBPeripheral *)peripheral fromCentralManager:(CBCentralManager *)centralManager;
- (instancetype)initWithPeripheral:(CBPeripheral *)peripheral fromCentralManager:(CBCentralManager *)centralManager;

#pragma mark - Actions

- (void)connect:(void (^)(NSError *error))callback;
- (void)disconnect:(void (^)(NSError *error))callback;
- (void)refreshRssi:(void (^)(NSNumber *rssi, NSError *error))callback;
- (void)discoverServices:(NSArray<CBUUID *> *)services callback:(void (^)(NSArray<CBService *> *services, NSError *error))callback;
- (void)discoverCharacteristics:(NSArray<CBUUID *> *)characteristics forService:(CBService *)service callback:(void (^)(NSArray<CBCharacteristic *> *characteristics, NSError *error))callback;
- (void)readValueForCharacteristic:(CBCharacteristic *)characteristic callback:(void (^)(NSData *value))callback;
- (id)subscribeForNotificationsFromCharacteristic:(CBCharacteristic *)characteristic notification:(void (^)(NSData *value))notification;
- (void)unsubscribeForNotificationsFromCharacteristic:(CBCharacteristic *)characteristic identifier:(id)identifier;

@end
