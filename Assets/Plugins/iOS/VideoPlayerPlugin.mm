#include "Unity/VideoPlayer.h"
#include "iPhone_View.h"

#import <UIKit/UIKit.h>

#include <stdlib.h>
#include <string.h>
#include <stdint.h>

extern "C" __attribute__((visibility ("default"))) NSString *const kUnityViewDidRotate;

@interface VideoPlayerInterface : NSObject <VideoPlayerDelegate> {
@public
    VideoPlayer *player;
    VideoPlayerView *view;
    CGRect margin;
}
- (void)playVideo:(NSURL *)videoURL;

- (void)orientationDidChange:(NSNotification *)notification;

- (void)onPlayerReady;

- (void)onPlayerDidFinishPlayingVideo;

@end

@implementation VideoPlayerInterface
- (void)playVideo:(NSURL *)videoURL {

    [player loadVideo:videoURL];

    if (!view && [player readyToPlay])
        [self play];
}

- (void)orientationDidChange:(NSNotification *)notification {
}

- (void)onPlayerReady {
    if (!player.isPlaying) {
        CGFloat scale = UnityGetGLView().contentScaleFactor;
        UIDeviceOrientation orientation = [[UIDevice currentDevice] orientation];
        CGRect bounds;

        if (orientation) {
            bounds.size.width = UnityGetGLView().bounds.size.width - (margin.origin.x + margin.size.width) / scale;
            bounds.size.height = UnityGetGLView().bounds.size.height - (margin.origin.y + margin.size.height) / scale;
        } else {
            bounds.size.width = UnityGetGLView().bounds.size.height - (margin.origin.x + margin.size.width) / scale;
            bounds.size.height = UnityGetGLView().bounds.size.width - (margin.origin.y + margin.size.height) / scale;
        }

        view.bounds = bounds;
        view.center = CGPointMake(view.bounds.size.width / 2 + margin.origin.x / scale, view.bounds.size.height / 2 + margin.origin.y / scale);
        [self play];
    }
}


- (void)play {
    if (view) {
        view.hidden = NO;
        [player playToView:view];
    } else {
        [player playToTexture];
    }
}

- (void)unload {
    if (view) {
        [view removeFromSuperview];
        view = nil;
    }

    [player unloadPlayer];
}

- (void)onPlayerDidFinishPlayingVideo {
    [self unload];
}
@end

static VideoPlayerInterface *_Player = nil;

static VideoPlayerInterface *_GetPlayer() {

    if (!_Player) {
        _Player = [[VideoPlayerInterface alloc] init];
        _Player->player = [[VideoPlayer alloc] init];
        _Player->player.delegate = _Player.self;
    }

    if (!_Player->player) {
        _Player->player = [[VideoPlayer alloc] init];
        _Player->player.delegate = _Player->player.self;
    }

    return _Player;
}

static NSURL *_GetUrl(const char *videoURL) {
    NSURL *url = nil;
    if (::strstr(videoURL, "://"))
        url = [NSURL URLWithString:[NSString stringWithUTF8String:videoURL]];
    else
        url = [NSURL fileURLWithPath:[[[[NSBundle mainBundle] bundlePath] stringByAppendingPathComponent:@"Data/Raw/"] stringByAppendingPathComponent:[NSString stringWithUTF8String:videoURL]]];
    return url;
}

extern "C" void PlayVideoView(int left, int top, int right, int bottom, const char *videoURL) {

    if (!_GetPlayer()->view) {
        _GetPlayer()->view = [[VideoPlayerView alloc] initWithFrame:UnityGetGLView().frame];
        [UnityGetGLView() addSubview:_GetPlayer()->view];
    }
    _GetPlayer()->margin = CGRectMake((CGFloat) left, (CGFloat) top, (CGFloat) right, (CGFloat) bottom);
    [[NSNotificationCenter defaultCenter] addObserver:_GetPlayer() selector:@selector(orientationDidChange:) name:kUnityViewDidRotate object:nil];
    [_GetPlayer() playVideo:_GetUrl(videoURL)];
}

extern "C" void PlayVideo(const char *videoURL) {
    if (_GetPlayer()->player.isPlaying) {
        [_GetPlayer()->player unloadPlayer];
    }
    [_GetPlayer() playVideo:_GetUrl(videoURL)];
}
extern "C" void PauseVideo() {
    [_GetPlayer()->player pause];
}

extern "C" void ResumeVideo() {
    [_GetPlayer()->player resume];
}

extern "C" void RewindVideo() {
    if (_GetPlayer()->view) {
        [_GetPlayer()->player rewind];
    } else {
        //FIXME TextureでRewindすると既に読み込まれたものは表示されないのでUnity側でRewindは行わないようにしている
    }
}
extern "C" bool CanOutputToTexture(const char *videoURL) {
    return [VideoPlayer CanPlayToTexture:_GetUrl(videoURL)];
}

extern "C" bool PlayerReady() {
    return [_GetPlayer()->player readyToPlay];
}

extern "C" float DurationSeconds() {
    return [_GetPlayer()->player durationSeconds];
}

extern "C" void Extents(int *w, int *h) {
    CGSize sz = [_GetPlayer()->player videoSize];
    *w = (int) sz.width;
    *h = (int) sz.height;
}

extern "C" int CurFrameTexture() {
    return [_GetPlayer()->player curFrameTexture];
}

extern "C" void SeekToVideo(float time) {
    [_GetPlayer()->player seekTo:time];
}

extern "C" bool IsPlaying() {
    if (!_GetPlayer()->player)return false;
    return [_GetPlayer()->player isPlaying];
}

extern "C" void StopVideo() {
    if (_GetPlayer()->player) {
        [_GetPlayer() unload];
    }
}