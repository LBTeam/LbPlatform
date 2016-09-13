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
	
	/**
	 * 新增节点
	 */
	public function add(){
		$node_model = D("Node");
		$pid  = I('get.pid',0);
		if(IS_POST){
			$rules = array(
				array('title','require','标题不能为空！'),
				array('title','','标题已经存在！',0,'unique',1),
				array('name','require','英文标识不能为空！')
			);
            $data = array();
			$data['name'] = I("post.name", "");
			$data['title'] = I("post.title", "");
			$data['status'] = I("post.status", 0);
			$data['remark'] = I("post.remark", "");
			$data['sort'] = I("post.sort", 1);
			$data['pid'] = I("post.pid", 0);
			$data['level'] = I("post.level", 0);
			if($node_model->validate($rules)->create($data)){
				$id = $node_model->add();
				if($id){
                    $this->success('新增成功', U('Index/Node/index', array('pid'=>$data['pid'])));
                } else {
                    $this->error('新增失败');
                }
			}else{
				$this->error($node_model->getError());
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
            $this->meta_title = '新增节点';
            $this->display();
        }
	}
}