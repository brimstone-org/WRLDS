#ifndef __WRLDSBALLPLUGIN_H__
#define __WRLDSBALLPLUGIN_H__

#ifdef __cplusplus
extern "C" {
#endif
    
#pragma mark - Lifecycle
    void initializer();
    void finalizer();
    
#pragma mark - Interaction Delegate
    typedef void (*BALL_DID_BOUNCE_CALLBACK)(int type, float sumG);
    typedef void (*BALL_DID_SHAKE_CALLBACK)(double shakeProgress);
    typedef void (*BALL_DID_RECEIVE_FIFO_DATA_CALLBACK)(float* fifoData, int length);
    typedef void (*BALL_DID_CHANGE_CONNECTION_STATE_CALLBACK)(int state, char* stateMessage);
    void set_interaction_delegate_ball_did_bounce_callback(BALL_DID_BOUNCE_CALLBACK callback);
    void set_interaction_delegate_ball_did_shake_callback(BALL_DID_SHAKE_CALLBACK callback);
    void set_interaction_delegate_ball_did_receive_fifo_data_callback(BALL_DID_RECEIVE_FIFO_DATA_CALLBACK callback);
    void set_connection_delegate_ball_did_change_state_callback(BALL_DID_CHANGE_CONNECTION_STATE_CALLBACK callback);
    
#pragma mark - Properties
    void set_low_threshold(double threshold);
    void set_high_threshold(double threshold);
    void set_shake_interval(double intervalSeconds);
    char * get_connection_state();
    char * get_device_address();
    
#pragma mark - Actions
    void start_scanning();
    void stop_scanning();
    void connect_to_ball(char *address);
    
    
#ifdef __cplusplus
}
#endif

#endif
