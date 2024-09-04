//
//  WRLDSBluetoothScanner.h
//  WRLDSSDK
//
//  Created by Patrik Nyblad on 2018-07-05.
//

#import <Foundation/Foundation.h>
#import "WRLDSBluetoothDevice.h"

@class WRLDSBluetoothScanner;

@protocol WRLDSBluetoothScannerDelegate <NSObject>
@optional

/**
 Scanning started.
 @property scanner The scanner that started scanning.
 */
- (void)scannerDidStartScanning:(WRLDSBluetoothScanner *)scanner;

/**
 Scanning stopped.
 @property scanner The scanner that stopped scanning. This could be due to timeout, user action etc.
 */
- (void)scannerDidStopScanning:(WRLDSBluetoothScanner *)scanner;

/**
 Triggered multiple times with whatever nearby devices we can find that matches our search criteria.
 The delegate method can be called for the same device when info about the device changes. To get a
 full list of all scanned devices use the -scannedDevices property.
 @property scanner The scanner that found devices.
 @property device A device that was just discovered or its data was updated.
 */
- (void)scanner:(WRLDSBluetoothScanner *)scanner didDiscoverDevice:(WRLDSBluetoothDevice *)device;

/**
 An error occured, this error might not be in direct response to an action so it makes sense for you
 to listen for this even if you implement all blocks based apis.
 */
- (void)scanner:(WRLDSBluetoothScanner *)scanner didFailWithError:(NSError *)error;


@end


@interface WRLDSBluetoothScanner : NSObject

@property (nonatomic, weak) id<WRLDSBluetoothScannerDelegate> delegate;

/**
 Decides how long the scanner continues to find devices.
 */
@property (nonatomic, assign) NSTimeInterval scanTimeout;

/**
 Allows you to add a filtering block that will be triggered whenever a list of devies are available so
 that you can filter out the ones you do not want to show.
 @param device The device currently up for filtering.
 @return YES if device is should be accepted or NO if device should not be accepted.
 */
@property (nonatomic, copy) BOOL (^filter)(WRLDSBluetoothDevice *device);

/**
 Devices found during scanning. These are only available while scanning occurs, if you want to connect
 to any of them you will have to store a reference to that object before you stop scanning.
 */
@property (nonatomic, strong, readonly) NSArray<WRLDSBluetoothDevice *> *foundDevices;

/**
 Setting this allows you to decide what order the devices are listed in when returned by the delegate.
 */
@property (nonatomic, copy) NSComparisonResult (^deviceSorting)(WRLDSBluetoothDevice *device1, WRLDSBluetoothDevice *device2);

- (void)startScanningForPeripheralsWithServices:(NSArray<CBUUID *> *)services;
- (void)stopScanning;

@end
