//
//  WRLDSConnectableDevice.h
//  WRLDSSDK
//
//  Created by Patrik Nyblad on 2018-07-18.
//  Copyright © 2018 WRLDS. All rights reserved.
//

#import <Foundation/Foundation.h>

/*
 A device that is found when scanning from WRLDSBall.
 An instance of this object can be used for connecting to a physical ball.
 */
@interface WRLDSConnectableDevice : NSObject

@property (nonatomic, strong, readonly) NSUUID *identifier;
@property (nonatomic, strong, readonly) NSNumber *rssi;
@property (nonatomic, strong, readonly) NSString *name;

@end
