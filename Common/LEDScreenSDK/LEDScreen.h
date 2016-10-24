#pragma once
#ifndef __LEDSCREEN_H__
#define __LEDSCREEN_H__

extern "C"
{
#include "libavcodec\avcodec.h"
#include "libavformat\avformat.h"
#include "libavutil\frame.h"
#include "libavutil\channel_layout.h"
#include "libavutil\common.h"
#include "libavutil\imgutils.h"
#include "libswscale\swscale.h" 
#include "libavutil\imgutils.h"    
#include "libavutil\opt.h"       
#include "libavutil\mathematics.h"    
#include "libavutil\samplefmt.h" 
#include "libswscale\swscale.h"
};
#pragma comment(lib, "avcodec.lib")
#pragma comment(lib, "avformat.lib")
#pragma comment(lib, "avdevice.lib")
#pragma comment(lib, "avfilter.lib")
#pragma comment(lib, "avutil.lib")
#pragma comment(lib, "postproc.lib")
#pragma comment(lib, "swresample.lib")
#pragma comment(lib, "swscale.lib")

#include <iostream>
#include <SDL.h>
#include "SDL_image.h"

using namespace System;
using namespace Runtime::InteropServices;

public ref class LEDScreen
{
public:
	LEDScreen(int x, int y, int width, int height)
		:x_(x), y_(y), width_(width), height_(height)
	{ }
		
	bool IsShown();
	void Focus();
	void Free();
	void PlayImage(System::String ^fileName);
	void PlayVideo(System::String ^fileName);

	bool HasMouseFocus();
	bool HasKeyboardFocus();
	bool IsMinimized();

private:
	SDL_Window *window_;
	SDL_Renderer *renderer_;
	SDL_Texture *texture_;
	int windowid_;

	int width_;
	int height_;
	int x_;
	int y_;

	bool shown_;
	bool mousefocus_;
	bool keyboardfocus_;
	bool fullscreen_;
	bool minimized_;
private:
	bool Init(System::String ^fileName);
	void HandleEvent(SDL_Event &e);
	void Render();
};


#endif