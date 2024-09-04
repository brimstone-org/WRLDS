//
//  WRLDSSDK.h
//  WRLDSSDK
//
//  Welcome to the wonderful, amazing, totally crazy good wrlds SDK!<br/>
//  This framework will help your App to connect with the wrlds ball and receive events during
//  interaction.
//
//  Created by Patrik Nyblad on 2018-07-10.
//  Copyright © 2018 WRLDS. All rights reserved.
//

#include <TargetConditionals.h>
#import <Foundation/Foundation.h>

//! Project version number for WRLDSSDK.
FOUNDATION_EXPORT double WRLDSSDKVersionNumber;

//! Project version string for WRLDSSDK.
FOUNDATION_EXPORT const unsigned char WRLDSSDKVersionString[];

// In this header, you should import all the public headers of your framework using statements like #import <WRLDSSDK/PublicHeader.h>

#import "WRLDSBall.h"
#import "WRLDSConnectableDevice.h"

#if (TARGET_OS_IPHONE || TARGET_OS_SIMULATOR)
#import "WRLDSScanViewController.h"
#endif
