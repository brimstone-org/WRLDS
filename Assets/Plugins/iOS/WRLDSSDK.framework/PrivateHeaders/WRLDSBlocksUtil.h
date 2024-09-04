//
//  WRLDSBlocksUtil.h
//  WRLDSSDK
//
//  Created by Patrik Nyblad on 2018-07-24.
//  Copyright © 2018 WRLDS. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface WRLDSBlocksUtil : NSObject

/**
 Calls the passed block with the passed args.
 */
+ (void)runBlock:(id)block withArgs:(NSArray *)args;


@end
