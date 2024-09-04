//
//  WRLDS.h
//  WRLDSSDK
//
//  Welcome to the wonderful, amazing, totally crazy good wrlds SDK!<br/>
//  This framework will help your App to connect with the wrlds ball and receive events during
//  interaction.
//
//  Created by Patrik Nyblad on 2018-07-03.
//  Copyright © 2018 WRLDS. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "WRLDSConnectableDevice.h"

@class WRLDSBall;

/*
 The bounce strength measured when the ball is bounced.
 */
typedef NS_ENUM(NSInteger, WRLDSBallBounceStrength) {
    WRLDSBallBounceStrengthSoft,
    WRLDSBallBounceStrengthNormal,
    WRLDSBallBounceStrengthHard
};


/*
 Different states that the connection can be in.
 This tries to align the int value with the number used in Android but is not guaranteed.
 */
typedef NS_ENUM(NSInteger, WRLDSConnectionState) {
    WRLDSConnectionStateError = -1,
    WRLDSConnectionStateDisconnecting = 3,
    WRLDSConnectionStateDisconnected,
    WRLDSConnectionStateConnecting,
    WRLDSConnectionStateConnected,
    WRLDSConnectionStateScanningStarted,
    WRLDSConnectionStateScanningFinished,
    WRLDSConnectionStateConnectionFailed,
    WRLDSConnectionStateScanningCanceled
};

/**
 A delegate to respond to connection events to let you know about new devices and current connection.
 */
@protocol WRLDSBallConnectionDelegate <NSObject>
@optional

/**
 Triggered when the connection state changes for example:<br/>
 - When the BLE service is started and stopped.
 - When a method related to BLE connectivity is invoked.<br/>
 - When a process related to BLE connectivity method finalizes.<br/>
 - When an automatic connection change occur.<br/>
 @param ball The ball that changed state.
 @param connectionState Signify the resulting state.
 @param stateMessage Most of the time the state type is self explanatory, but this
                      parameter can contain a clear text message that can give more insight
                      into the cause of the state change.
 */
- (void)ball:(WRLDSBall *)ball didChangeConnectionState:(WRLDSConnectionState)connectionState message:(NSString *)stateMessage;

- (void)ball:(WRLDSBall *)ball didDiscoverConnectableDevice:(WRLDSConnectableDevice *)bluetoothDevice;

@end

@protocol WRLDSBallInteractionDelegate <NSObject>
@optional

/**
 Triggered when a bounce is detected by the ball firmware.
 @param ball The ball that bounced.
 @param bounceStrength The interpreted bounce strength.
 @param totalforce Normalized total force that triggered the bounce event.
 */
- (void)ball:(WRLDSBall *)ball didBounce:(WRLDSBallBounceStrength)bounceStrength withTotalForce:(double)totalforce;

/**
 Triggered when a shake action is detected.
 @param ball The ball that was shaked.
 @param shakeProgress float between 0.0 and 1.0 indicating in percentage the progress
 towards a predefined amount of shaking that have been achieved.
 */
- (void)ball:(WRLDSBall *)ball didShake:(double)shakeProgress;

/**
 Triggered together with onBounce but exposes the 10 closest normalised accelerometer data
 points around the highest G data point.
 @param ball The ball that was moved to create the event.
 @param fifoData The 10 closest normalised accelerometer data points around the highest G
 data point
 */
- (void)ball:(WRLDSBall *)ball didReceiveFifoData:(float *)fifoData withLength:(int)length;

/**
 Triggered together with onBounce but expose the full accelerometer data
 points from the FIFO.
 @param ball The ball that was moved to create the event.
 @param fifoData Full accelerometer data points from the FIFO.
 */
- (void)ball:(WRLDSBall *)ball didReceiveHighResFifoData:(float[])fifoData;

@end

/**
 A representation of a WRLDS ball, each instance can be connected to a single ball and will
 communicate with you through a connectionDelegate for connection events and delegate for
 interaction events.
 */
@interface WRLDSBall : NSObject

/**
 The receiver responsible for handling interaction events.
 */
@property (nonatomic, weak) id<WRLDSBallInteractionDelegate> delegate;

/**
 The receiver responsible for connection events.
 */
@property (nonatomic, weak) id<WRLDSBallConnectionDelegate> connectionDelegate;

/**
 Any value below the set threshold value will be considered a "soft" bounce event.
 A bounce harder than this but weaker than -highThreshold is considered a "normal" bounce.
 @see highThreshold
 @default 30.0
 */
@property (nonatomic, assign) double lowThreshold; // Default 30.0

/**
 Any value above the threshold value will be considered a "hard" bounce event.
 A bounce softer than this but harder than -lowThreshold is considered a "normal" bounce.
 @see lowThreshold
 @default 60.0
 */
@property (nonatomic, assign) double highThreshold;

/**
 The time interval used to check for a shake gesture in seconds.
 @default 0.250
 */
@property (nonatomic, assign) NSTimeInterval shakeInterval;

/**
 This filter can be used to scan only for devices with names that contain this string.
 */
@property (nonatomic, strong) NSString *deviceNameFilter;

/**
 The current state of the connection with the physical ball.
 */
@property (nonatomic, assign, readonly) WRLDSConnectionState connectionState;

/**
 A list of found devices during a scanning session
 */
@property (nonatomic, strong, readonly) NSArray<WRLDSConnectableDevice *> *deviceList;


/**
 Get the name of the connected ball, will give you nil if no ball is connected.
 Setting this property will change the bluetooth name of the ball.
 @warning Setting this does not have any effect at the moment due to incompatibilities
 between the ball firmware and iOS.
 */
@property (nonatomic, strong) NSString *deviceName;

/**
 Get the device address (MAC) of the connected device.
 */
@property (nonatomic, strong, readonly) NSUUID *deviceAddress;

/**
 The last recorded battery level of the device.
 @return int 0-100 The battery level between 0-100% that was measured at wake up.
 */
@property (nonatomic, assign, readonly) int batteryLevel;

/**
 Signal strength described in db.
 */
@property (nonatomic, assign, readonly) NSNumber *rssiLevel;


#pragma mark - Actions

/**
 Start scanning for connectable WRLDS balls. All found devices will be available through callbacks
 to connectionDelegate and the local -deviceList property.
 Scanning will be performed for a set period of time but can be interrupted by calling the
 stopScanning method.
 You should start connecting to a device before you stop scanning since the -deviceList property
 will be cleared as soon as -stopScanning is called.
 */
- (void)startScanning;


/**
 Stop scanning for BLE devices. The list of devices to choose from will be cleared and you have
 to search again if you did not connect to the device you wanted before calling -stopScanning.
 */
- (void)stopScanning;


/**
 Connect to a device found while scanning.
 @param connectableDevice A device you retreived in your connectionDelegate while scanning that
 you want to connect to.
 @param completion An asynchronous block that will be triggered when the connection has finished,
 an error object is passed if it failed.
 */
- (void)connectToBall:(WRLDSConnectableDevice *)connectableDevice completion:(void (^)(NSError *error))completion;

/**
 Connect to a ball with a specific UUID, you can use this method only if you know what UUID the
 ball has from earlier.
 @param deviceId The unique ID for the ball you want to connect to.
 @param timeout The number of seconds you want to try to connect to the ball.
 @param completion Get notified when connection is complete or failed.
 */
- (void)connectToBallWithUniqueID:(NSUUID *)deviceId timeout:(NSTimeInterval)timeout completion:(void (^)(NSError *error))completion;

/**
 Disconnect from the current device.
 */
- (void)disconnectFromBall;


@end
