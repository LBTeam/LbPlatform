<?php
namespace Index\Controller;

class UserController extends CommonController {
    public function index(){
    	$this->meta_title = '用户列表';
		$this->display();
    }
}