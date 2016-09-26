<?php
namespace Index\Controller;

class IndexController extends CommonController {
    public function index(){
        $this->meta_title = '管理首页';
        $this->display();
    }
    
    public function child_city($id=0){
    	$region_model = D("Region");
    	$citys = array();
    	if($id){
    		$citys = $region_model->all_region($id);
    	}
    	$this->success($citys);
    }
}