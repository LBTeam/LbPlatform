<?php
/**
 * 节点管理控制器
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Index\Controller;
use Index\Service\TreeService;

class NodeController extends CommonController
{
	/**
	 * 节点列表
	 */
	public function index(){
		$node_model = D("Node");
		$pid  = I('get.pid',0);
		if($pid){
            $data = $node_model->node_by_id($pid);
            $this->assign('data',$data);
        }
		$titles = $node_model->all_node_title();
		$list = $node_model->all_node_by_pid($pid);
		int_to_string($list,array('level'=>array(1=>'模块',2=>'列表',3=>'操作'),'status'=>array(1=>'关闭',0=>'开启')));
		if($list) {
            foreach($list as &$key){
                if($key['pid']){
                    $key['up_title'] = $titles[$key['pid']];
                }
            }
            $this->assign('list',$list);
        }
		$this->meta_title = '节点列表';
        $this->display();
	}
	
	public function add(){
		$node_model = D("Node");
		$pid  = I('get.pid',0);
		if(IS_POST){
            $Menu = D('Menu');
            $data = $Menu->create();
            if($data){
                $id = $Menu->add();
                if($id){
                    // S('DB_CONFIG_DATA',null);
                    //记录行为
                    action_log('update_menu', 'Menu', $id, UID);
                    $this->success('新增成功', Cookie('__forward__'));
                } else {
                    $this->error('新增失败');
                }
            } else {
                $this->error($Menu->getError());
            }
        } else {
        	$TreeService = new TreeService();
			if($pid){
				$level = $node_model->node_by_id($pid, 'level');
				$this->assign('level', $level['level']+1);
			}
            $menus = $node_model->get_all_menu_nodes();
            $menus = $TreeService->toFormatTree($menus);
            $menus = array_merge(array(0=>array('id'=>0,'title_show'=>'顶级菜单')), $menus);
            $this->assign('Menus', $menus);
			$this->assign('pid', $pid);
            $this->meta_title = '新增菜单';
            $this->display();
        }
	}
}