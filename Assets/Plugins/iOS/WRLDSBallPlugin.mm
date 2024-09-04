#include "WRLDSBallPlugin.h"
#import <Foundation/Foundation.h>
#import <WRLDSSDK/WRLDSSDK.h>

#define EXPORT __attribute__((visibility("default")))

#pragma mark - Helpers

@interface WRLDSBallUnityPlugin : NSObject <WRLDSBallInteractionDelegate, WRLDSBallConnectionDelegate>

@property (nonatomic, strong) WRLDSBall *ball;
@property (nonatomic, strong) WRLDSScanViewController *scanViewController;

@end

@implementation WRLDSBallUnityPlugin

+ (instancetype)sharedInstance {
    static WRLDSBallUnityPlugin *sharedInstance = nil;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        sharedInstance = [[WRLDSBallUnityPlugin alloc] init];
    });
    return sharedInstance;
}

- (instancetype)init {
    self = [super init];
    if (self) {
        _ball = [[WRLDSBall alloc] init];
        _ball.delegate = self;
        _scanViewController = [[WRLDSScanViewController alloc] initWithBall: _ball];
        _scanViewController.connectionDelegate = self;
    }
    return self;
}

WRLDSBall *_ball;
WRLDSScanViewController *_scanViewController;

BALL_DID_BOUNCE_CALLBACK ballDidBounceCallback = NULL;
BALL_DID_SHAKE_CALLBACK ballDidShakeCallback = NULL;
BALL_DID_CHANGE_CONNECTION_STATE_CALLBACK ballDidChangeConnectionStateCallback = NULL;
BALL_DID_RECEIVE_FIFO_DATA_CALLBACK ballDidReceiveFifoDataCallback = NULL;

#pragma mark Life cycle
// __attribute__((constructor))
EXPORT
void initializer(void)
{
    NSLog(@"WRLDSBallPlugin: initializer");
    // establish any other global resources here
    [WRLDSBallUnityPlugin sharedInstance];
}

//__attribute__((destructor))
EXPORT
void finalizer(void)
{
    NSLog(@"WRLDSBallPlugin: finalizer");
    // free up any other global resources here
}


#pragma mark - Interaction Delegate

EXPORT
void set_interaction_delegate_ball_did_bounce_callback(BALL_DID_BOUNCE_CALLBACK callback) {
    ballDidBounceCallback = callback;
}

EXPORT
void set_interaction_delegate_ball_did_shake_callback(BALL_DID_SHAKE_CALLBACK callback) {
    ballDidShakeCallback = callback;
}

EXPORT
void set_interaction_delegate_ball_did_receive_fifo_data_callback(BALL_DID_RECEIVE_FIFO_DATA_CALLBACK callback) {
    ballDidReceiveFifoDataCallback = callback;
}

#pragma mark - Connection Delegate

EXPORT
void set_connection_delegate_ball_did_change_state_callback(BALL_DID_CHANGE_CONNECTION_STATE_CALLBACK callback) {
    ballDidChangeConnectionStateCallback = callback;
}

#pragma mark - Properties

EXPORT
void set_low_threshold(double threshold) {
    [[[WRLDSBallUnityPlugin sharedInstance] ball] setLowThreshold:threshold];
}

EXPORT
void set_high_threshold(double threshold) {
    [[[WRLDSBallUnityPlugin sharedInstance] ball] setHighThreshold:threshold];
}

EXPORT
void set_shake_interval(double intervalSeconds) {
    [[[WRLDSBallUnityPlugin sharedInstance] ball] setShakeInterval:(NSTimeInterval)intervalSeconds];
}

EXPORT
char * get_connection_state() {
    WRLDSConnectionState state = [[[WRLDSBallUnityPlugin sharedInstance] ball] connectionState];
    return (char*)state;
}

EXPORT
char * get_device_address() {
    NSString *address = [[[[WRLDSBallUnityPlugin sharedInstance] ball] deviceAddress] UUIDString];
    return (char*) cStringCopy([address UTF8String]);
}


#pragma mark - Actions

EXPORT
void start_scanning() {
    [[[WRLDSBallUnityPlugin sharedInstance] scanViewController] startScanning];
}

EXPORT
void stop_scanning() {
    [[[WRLDSBallUnityPlugin sharedInstance] ball] stopScanning];
}

EXPORT
void connect_to_ball(char *address) {
    NSUUID *uuid = [[NSUUID alloc] initWithUUIDString:[NSString stringWithUTF8String:address]];
    [[[WRLDSBallUnityPlugin sharedInstance] ball] connectToBallWithUniqueID:uuid timeout:5.0 completion:nil];
}

char* cStringCopy(const char* string)
{
    if (string == NULL)
        return NULL;
    
    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    
    return res;
}

#pragma mark - WRLDSConnectionDelegate
- (void)ball:(WRLDSBall *)ball didChangeConnectionState:(WRLDSConnectionState)connectionState message:(NSString *)stateMessage {
    NSLog(@"Plugin connection state change.");
    if (ballDidChangeConnectionStateCallback) {
        ballDidChangeConnectionStateCallback(connectionState, cStringCopy([stateMessage UTF8String]));
    }
}

#pragma mark - WRLDSInteractionDelegate

- (void)ball:(WRLDSBall *)ball didBounce:(WRLDSBallBounceStrength)bounceStrength withTotalForce:(double)totalForce {
    NSLog(@"Plugin bounce.");
    if (ballDidBounceCallback) {
        ballDidBounceCallback(bounceStrength, totalForce);
    }
}

- (void)ball:(WRLDSBall *)ball didReceiveFifoData:(NSData *)fifoData withLength:(int)length; {
    NSLog(@"Plugin fifo data.");
    if (ballDidReceiveFifoDataCallback) {
        ballDidReceiveFifoDataCallback(fifoData, length);
    }
}

@end


