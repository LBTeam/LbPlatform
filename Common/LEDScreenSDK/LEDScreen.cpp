#include "stdafx.h"
#include "LEDScreen.h"
#include <stdio.h>
#include <sstream>



int decode(AVCodecContext *avctx, AVFrame *frame, int *got_frame, AVPacket *pkt)
{
	int ret;

	*got_frame = 0;

	if (pkt) {
		ret = avcodec_send_packet(avctx, pkt);
		// In particular, we don't expect AVERROR(EAGAIN), because we read all
		// decoded frames with avcodec_receive_frame() until done.
		if (ret < 0)
			return ret == AVERROR_EOF ? 0 : ret;
	}

	ret = avcodec_receive_frame(avctx, frame);
	if (ret < 0 && ret != AVERROR(EAGAIN) && ret != AVERROR_EOF)
		return ret;
	if (ret >= 0)
		*got_frame = 1;

	return 0;
}


bool LEDScreen::Init(System::String ^fileName)
{
	window_ = SDL_CreateWindow("", x_, y_, width_, height_, SDL_WINDOW_SHOWN | SDL_WINDOW_BORDERLESS);
	if (NULL != window_)
	{
		renderer_ = SDL_CreateRenderer(window_, -1, SDL_RENDERER_ACCELERATED | SDL_RENDERER_PRESENTVSYNC);
		if (NULL == renderer_)
		{
			printf("Renderer could not be created! SDL Error: %s\n", SDL_GetError());
			SDL_DestroyWindow(window_);
			window_ = NULL;
		}
		else
		{
			SDL_SetRenderDrawColor(renderer_, 0xFF, 0xFF, 0xFF, 0xFF);

			//Grab window identifier
			windowid_ = SDL_GetWindowID(window_);

			//Flag as opened
			shown_ = true;

			const char* chars = (const char*)(Marshal::StringToHGlobalAnsi(fileName)).ToPointer();
			texture_ = IMG_LoadTexture(renderer_, chars);
			Marshal::FreeHGlobal(IntPtr((void*)chars));
		}
	}
	else
	{
		printf("Window could not be created! SDL Error: %s\n", SDL_GetError());
	}

	return NULL != window_ && NULL != renderer_ && NULL != texture_;
}


bool LEDScreen::IsShown()
{
	return shown_;
}

void LEDScreen::HandleEvent(SDL_Event & e)
{
	if (e.type == SDL_WINDOWEVENT && e.window.windowID == windowid_)
	{
		//Caption update flag
		bool updateCaption = false;

		switch (e.window.event)
		{
			//Window appeared
		case SDL_WINDOWEVENT_SHOWN:
			shown_ = true;
			break;

			//Window disappeared
		case SDL_WINDOWEVENT_HIDDEN:
			shown_ = false;
			break;

			//Get new dimensions and repaint
		case SDL_WINDOWEVENT_SIZE_CHANGED:
			width_ = e.window.data1;
			height_ = e.window.data2;
			SDL_RenderPresent(renderer_);
			break;

			//Repaint on expose
		case SDL_WINDOWEVENT_EXPOSED:
			SDL_RenderPresent(renderer_);
			break;

			//Mouse enter
		case SDL_WINDOWEVENT_ENTER:
			mousefocus_ = true;
			updateCaption = true;
			break;

			//Mouse exit
		case SDL_WINDOWEVENT_LEAVE:
			mousefocus_ = false;
			updateCaption = true;
			break;

			//Keyboard focus gained
		case SDL_WINDOWEVENT_FOCUS_GAINED:
			keyboardfocus_ = true;
			updateCaption = true;
			break;

			//Keyboard focus lost
		case SDL_WINDOWEVENT_FOCUS_LOST:
			keyboardfocus_ = false;
			updateCaption = true;
			break;

			//Window minimized
		case SDL_WINDOWEVENT_MINIMIZED:
			minimized_ = true;
			break;

			//Window maxized
		case SDL_WINDOWEVENT_MAXIMIZED:
			minimized_ = false;
			break;

			//Window restored
		case SDL_WINDOWEVENT_RESTORED:
			minimized_ = false;
			break;

			//Hide on close
		case SDL_WINDOWEVENT_CLOSE:
			SDL_HideWindow(window_);
			break;
		}

		//Update window caption with new data
		if (updateCaption)
		{
			std::stringstream caption;
			caption << "SDL Tutorial - ID: " << windowid_ << " MouseFocus:" << ((mousefocus_) ? "On" : "Off") << " KeyboardFocus:" << ((keyboardfocus_) ? "On" : "Off");
			SDL_SetWindowTitle(window_, caption.str().c_str());
		}
	}
}

void LEDScreen::Free()
{
	if (window_ != NULL)
	{
		SDL_DestroyWindow(window_);
	}

	width_ = 0;
	height_ = 0;
	mousefocus_ = false;
	keyboardfocus_ = false;
}

void LEDScreen::Focus()
{
	if (!shown_)
	{
		SDL_ShowWindow(window_);
	}
	SDL_RaiseWindow(window_);
}

void LEDScreen::Render()
{
	if (!minimized_)
	{

		/* render background, whereas NULL for source and destination
		rectangles just means "use the default" */
		SDL_RenderClear(renderer_);
		//SDL_RenderCopy(renderer_, Background_Tx, NULL, NULL);

		/* render the current animation step of our shape */
		SDL_RenderCopy(renderer_, texture_, /*&SrcR*/NULL, /*&DestR*/ NULL);
		SDL_RenderPresent(renderer_);
		/*	}
		}*/
	}
}

bool LEDScreen::HasMouseFocus()
{
	return mousefocus_;
}

bool LEDScreen::HasKeyboardFocus()
{
	return keyboardfocus_;
}

bool LEDScreen::IsMinimized()
{
	return minimized_;
}

void LEDScreen::PlayImage(System::String ^fileName)
{

	bool success = true;

	//Initialize SDL
	if (SDL_Init(SDL_INIT_VIDEO) < 0)
	{
		printf("SDL could not initialize! SDL Error: %s\n", SDL_GetError());
		success = false;
	}
	else
	{
		//Set texture filtering to linear
		if (!SDL_SetHint(SDL_HINT_RENDER_SCALE_QUALITY, "1"))
		{
			printf("Warning: Linear texture filtering not enabled!");
		}

		//Create window
		if (!Init(fileName))
		{
			printf("Window 0 could not be created!\n");
			success = false;
		}
	}


	if (!success)
	{
		printf("Failed to initialize!\n");
	}
	else
	{
		//Main loop flag
		bool quit = false;

		//Event handler
		SDL_Event e;

		//While application is running
		while (!quit)
		{
			//Handle events on queue
			while (SDL_PollEvent(&e) != 0)
			{
				//User requests quit
				if (e.type == SDL_QUIT)
				{
					quit = true;
				}
				HandleEvent(e);

				//Pull up window
				if (e.type == SDL_KEYDOWN)
				{
					switch (e.key.keysym.sym)
					{
					case SDLK_1:
						Focus();
						break;

						//case SDLK_2:
						//	g_Screens[1].Focus();
						//	break;

						//case SDLK_3:
						//	g_Screens[2].Focus();
						//	break;
					}
				}
			}

			Render();

			//Check all windows
			bool allWindowsClosed = true;
			if (IsShown())
			{
				allWindowsClosed = false;
			}

			//Application closed all windows
			if (allWindowsClosed)
			{
				quit = true;
			}
		}
	}
}

void LEDScreen::PlayVideo(System::String ^fileName)
{
	AVFormatContext	*pFormatCtx;
	int				i, videoindex;
	AVCodecContext	*pCodecCtx;
	AVCodecParameters *pCodecParas;
	AVCodec			*pCodec;
	AVFrame	*pFrame, *pFrameYUV;
	uint8_t *out_buffer;
	AVPacket *packet;
	int y_size;
	int ret, got_picture;
	struct SwsContext *img_convert_ctx;

	SDL_Rect sdlRect;

	av_register_all();
	avformat_network_init();
	pFormatCtx = avformat_alloc_context();

	const char* filepath = (const char*)(Marshal::StringToHGlobalAnsi(fileName)).ToPointer();


	if (avformat_open_input(&pFormatCtx, filepath, NULL, NULL) != 0) {
		printf("Couldn't open input stream.\n");
	}
	if (avformat_find_stream_info(pFormatCtx, NULL) < 0) {
		printf("Couldn't find stream information.\n");
	}
	videoindex = -1;
	for (i = 0; i < pFormatCtx->nb_streams; i++)
		if (pFormatCtx->streams[i]->codecpar->codec_type == AVMEDIA_TYPE_VIDEO) {
			videoindex = i;
			break;
		}
	if (videoindex == -1) {
		printf("Didn't find a video stream.\n");
	}

	pCodecParas = pFormatCtx->streams[videoindex]->codecpar;
	pCodec = avcodec_find_decoder(pCodecParas->codec_id);
	if (pCodec == NULL) {
		printf("Codec not found.\n");
	}
	pCodecCtx = avcodec_alloc_context3(pCodec);
	if (avcodec_parameters_to_context(pCodecCtx, pCodecParas) < 0) {
		printf("Couldn't Convert to context");
	}

	if (avcodec_open2(pCodecCtx, pCodec, NULL) < 0) {
		printf("Could not open codec.\n");
	}

	pFrame = av_frame_alloc();
	pFrameYUV = av_frame_alloc();
	out_buffer = (uint8_t *)av_malloc(av_image_get_buffer_size(AV_PIX_FMT_YUV420P, width_, height_, 16));
	if (!out_buffer)
	{
		av_free(pFrame);
		av_free(pFrameYUV);
	}
	av_image_fill_arrays(pFrameYUV->data, pFrameYUV->linesize, out_buffer, AV_PIX_FMT_YUV420P, width_, height_, 1);
	packet = (AVPacket *)av_malloc(sizeof(AVPacket));
	//Output Info-----------------------------
	printf("--------------- File Information ----------------\n");
	av_dump_format(pFormatCtx, 0, filepath, 0);
	printf("-------------------------------------------------\n");
	img_convert_ctx = sws_getContext(pCodecCtx->width, pCodecCtx->height, pCodecCtx->pix_fmt,
		width_, height_, AV_PIX_FMT_YUV420P, SWS_POINT, NULL, NULL, NULL);

#if OUTPUT_YUV420P 
	fp_yuv = fopen("output.yuv", "wb+");
#endif  

	if (SDL_Init(SDL_INIT_VIDEO | SDL_INIT_AUDIO | SDL_INIT_TIMER)) {
		printf("Could not initialize SDL - %s\n", SDL_GetError());
	}

	window_ = SDL_CreateWindow("Simplest ffmpeg player's Window", x_, y_,
		width_, height_,
		SDL_WINDOW_OPENGL| SDL_WINDOW_BORDERLESS);

	if (!window_) {
		printf("SDL: could not create window - exiting:%s\n", SDL_GetError());
	}

	renderer_ = SDL_CreateRenderer(window_, -1, 0);
	//IYUV: Y + U + V  (3 planes)
	//YV12: Y + V + U  (3 planes)
	texture_ = SDL_CreateTexture(renderer_, SDL_PIXELFORMAT_IYUV, SDL_TEXTUREACCESS_STREAMING, width_, height_);

	sdlRect.x = 0;
	sdlRect.y = 0;
	sdlRect.w = width_;
	sdlRect.h = height_;

	//SDL End----------------------
	while (av_read_frame(pFormatCtx, packet) >= 0) {
		if (packet->stream_index == videoindex) {
			//ret = avcodec_decode_video2(pCodecCtx, pFrame, &got_picture, packet);
			ret = decode(pCodecCtx, pFrame, &got_picture, packet);
			if (ret < 0) {
				printf("Decode Error.\n");
			}
			if (got_picture) {
				sws_scale(img_convert_ctx, (const uint8_t* const*)pFrame->data, pFrame->linesize, 0, pCodecCtx->height,
					pFrameYUV->data, pFrameYUV->linesize);

#if OUTPUT_YUV420P
				y_size = pCodecCtx->width*pCodecCtx->height;
				fwrite(pFrameYUV->data[0], 1, y_size, fp_yuv);    //Y 
				fwrite(pFrameYUV->data[1], 1, y_size / 4, fp_yuv);  //U
				fwrite(pFrameYUV->data[2], 1, y_size / 4, fp_yuv);  //V
#endif
																	//SDL---------------------------
#if 0
				SDL_UpdateTexture(sdlTexture, NULL, pFrameYUV->data[0], pFrameYUV->linesize[0]);
#else
				SDL_UpdateYUVTexture(texture_, &sdlRect,
					pFrameYUV->data[0], pFrameYUV->linesize[0],
					pFrameYUV->data[1], pFrameYUV->linesize[1],
					pFrameYUV->data[2], pFrameYUV->linesize[2]);
#endif	

				SDL_RenderClear(renderer_);
				SDL_RenderCopy(renderer_, texture_, NULL, &sdlRect);
				SDL_RenderPresent(renderer_);
				//SDL End-----------------------
				//Delay 40ms
				SDL_Delay(40);
			}
		}
		av_packet_unref(packet);
	}
	//flush decoder
	//FIX: Flush Frames remained in Codec
	while (1) {
		//ret = avcodec_decode_video2(pCodecCtx, pFrame, &got_picture, packet);  //deprecated
		ret = decode(pCodecCtx, pFrame, &got_picture, packet);
		if (ret < 0)
			break;
		if (!got_picture)
			break;
		sws_scale(img_convert_ctx, (const uint8_t* const*)pFrame->data, pFrame->linesize, 0, pCodecCtx->height,
			pFrameYUV->data, pFrameYUV->linesize);
#if OUTPUT_YUV420P
		int y_size = pCodecCtx->width*pCodecCtx->height;
		fwrite(pFrameYUV->data[0], 1, y_size, fp_yuv);    //Y 
		fwrite(pFrameYUV->data[1], 1, y_size / 4, fp_yuv);  //U
		fwrite(pFrameYUV->data[2], 1, y_size / 4, fp_yuv);  //V
#endif
															//SDL---------------------------
		SDL_UpdateTexture(texture_, NULL, pFrameYUV->data[0], pFrameYUV->linesize[0]);
		SDL_RenderClear(renderer_);
		SDL_RenderCopy(renderer_, texture_, NULL, NULL);
		SDL_RenderPresent(renderer_);
		//SDL End-----------------------
		//Delay 40ms
		SDL_Delay(40);
	}

	sws_freeContext(img_convert_ctx);

#if OUTPUT_YUV420P 
	fclose(fp_yuv);
#endif 

	SDL_Quit();

	av_frame_free(&pFrameYUV);
	av_frame_free(&pFrame);
	avcodec_close(pCodecCtx);
	avformat_close_input(&pFormatCtx);
	Marshal::FreeHGlobal(IntPtr((void*)filepath));
}

