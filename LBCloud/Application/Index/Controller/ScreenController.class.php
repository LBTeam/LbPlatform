<?php
/**
 * 屏幕管理controller
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Index\Controller;

class ScreenController extends CommonController
{
	public function index(){
		$led_model = D("Screen");
		$leds = $led_model->screen_by_uid(ADMIN_UID);
		dump($leds);
	}
}
