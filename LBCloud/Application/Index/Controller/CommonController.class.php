<?php
/**
 * 基类controller
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Index\Controller;
use Think\Controller;

class CommonController extends Controller
{
	protected function _initialize(){
		define('ADMIN_UID', is_login());
		if(!ADMIN_UID){// 还没登录 跳转到登录页面
            $this->redirect('Public/login');
        }
	}
}
