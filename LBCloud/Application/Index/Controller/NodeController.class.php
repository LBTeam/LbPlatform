<?php
/**
 * 节点管理控制器
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Index\Controller;

class NodeController extends CommonController
{
	public function index(){
		$node_model = D("Node");
		$pid  = I('get.pid',0);
		if($pid){
            $data = $node_model->node_by_id($pid);
            $this->assign('data',$data);
        }
		$titles = $node_model->all_node_title();
		$list = $node_model->all_node_by_pid($pid);
		if($list) {
            foreach($list as &$key){
                if($key['pid']){
                    $key['up_title'] = $titles[$key['pid']];
                }
            }
            $this->assign('list',$list);
        }
		$this->_nav = array(
			'Index' => '首页',
			'Node' => '节点管理',
			'index' => '节点列表'
		);
		$this->meta_title = '节点列表';
        $this->display();
	}
}