//
//  WRLDSScanViewController.h
//  WRLDSSDK
//
//  Created by Patrik Nyblad
//  Copyright © 2018 WRLDS. All rights reserved.
//

#include <TargetConditionals.h>

#if (TARGET_OS_IPHONE || TARGET_OS_SIMULATOR)

#import <UIKit/UIKit.h>
#import "WRLDSBall.h"

/**
 A view controller that can be presented manually or automatically to assist in letting the user
 select a compatible WRLDS ball device.
 
 Note: This instance registers itself as the connectionDelegate of the ball you initialize with.
 The WRLDSScanViewController will not function correctly if you remove it as connectionDelegate from the ball.
 To get connectionDelegate events please set your delegate on this class instead since all messages are forwarded.
 */
@interface WRLDSScanViewController : UIViewController

/**
 The receiver responsible for connection events.
 Events are forwarded from the ball instance to the delegate you set here.
 */
@property (nonatomic, weak) id<WRLDSBallConnectionDelegate> connectionDelegate;

/**
 The ball that this scan view controller is working on.
 */
@property (nonatomic, readonly) WRLDSBall *ball;

/**
 Designated initializer.
 This class must be initialized with a ball to perform its basic functionality.
 */
- (instancetype)initWithBall:(WRLDSBall *)ball;

/**
 Start scanning for WRLDS balls.
 Calling this method also presents UI to allow user.
 */
- (void)startScanning;

@end

#endif

