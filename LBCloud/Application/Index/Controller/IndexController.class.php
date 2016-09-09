<?php
namespace Index\Controller;

class IndexController extends CommonController {
    public function index(){
        $this->meta_title = '管理首页';
        $this->display();
    }
}