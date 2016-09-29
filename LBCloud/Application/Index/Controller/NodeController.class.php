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
				//array('title','','标题已经存在！',0,'unique',1),
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
            $menus = $TreeService->create_menu($menus);
            $menus = $TreeService->toFormatTree($menus);
            $menus = array_merge(array(0=>array('id'=>0,'title_show'=>'顶级菜单')), $menus);
            $this->assign('Menus', $menus);
			$this->assign('pid', $pid);
            $this->meta_title = '新增节点';
            $this->display();
        }
	}

	/**
	 * 修改节点
	 */
	public function edit($id = 0){
		$node_model = D('Node');
		if(IS_POST){
			$rules = array(
				array('title','require','标题不能为空！'),
				//array('title','','标题已经存在！',0,'unique',2),
				array('name','require','英文标识不能为空！')
			);
			$data = array();
			$data['id'] = I("post.id", "");
			$data['name'] = I("post.name", "");
			$data['title'] = I("post.title", "");
			$data['status'] = I("post.status", 0);
			$data['remark'] = I("post.remark", "");
			$data['sort'] = I("post.sort", 1);
			$data['pid'] = I("post.pid", 0);
			$data['level'] = I("post.level", 0);
            if($node_model->validate($rules)->create($data)){
                if($node_model->save()!== false){
                    $this->success('修改成功', U('Index/Node/index', array('pid'=>$data['pid'])));
                } else {
                    $this->error('修改失败');
                }
            } else {
                $this->error($node_model->getError());
            }
        } else {
            /* 获取数据 */
            $info = $node_model->node_by_id($id);
			if($info){
				$TreeService = new TreeService();
				$menus = $node_model->get_all_menu_nodes();
				$menus = $TreeService->create_menu($menus);
            	$menus = $TreeService->toFormatTree($menus);
				$menus = array_merge(array(0=>array('id'=>0,'title_show'=>'顶级菜单')), $menus);
				$this->assign('info', $info);
	            $this->meta_title = '编辑节点';
				$this->assign('Menus', $menus);
	            $this->display();
			}else{
				$this->error('获取节点信息错误');
			}
        }
	}

	/**
	 * 删除节点
	 */
	public function del(){
		$id = I('request.id', 0);
        if ( empty($id) ) {
            $this->error('请选择要操作的数据!');
        }
		$node_model = D('Node');
		$id = array_unique((array)$id);
        $map = array('id' => array('in', $id) );
        if($node_model->where($map)->delete()){
            $this->success('删除成功');
        } else {
            $this->error('删除失败！');
        }
	}
	
	/**
	 * 更改状态
	 */
	public function status($id,$value = 0){
        $id    = is_array($id) ? implode(',',$id) : $id;
        $where = array_merge( array('id' => array('in', $id )) ,array('id'=>$id) );
        $msg   = array_merge( array( 'success'=>'操作成功！', 'error'=>'操作失败！', 'url'=>'' ,'ajax'=>IS_AJAX) , (array)$msg );
		$data  = array('status'=>$value);
		$node_model = D("Node");
        if( $node_model->where($where)->save($data)!==false ) {
            $this->success($msg['success'],$msg['url'],$msg['ajax']);
        }else{
            $this->error($msg['error'],$msg['url'],$msg['ajax']);
        }
	}
	
	/**
	 * 排序
	 */
	public function sort(){
		if(IS_GET){
            $ids = I('get.ids');
            $pid = I('get.pid');

            //获取排序的数据
            $map = array('status'=>array('gt',-1));
            if(!empty($ids)){
                $map['id'] = array('in',$ids);
            }else{
                if($pid !== ''){
                    $map['pid'] = $pid;
                }
            }
			$node_model = D("Node");
            $list = $node_model->where($map)->field('id,title')->order('sort asc,id asc')->select();

            $this->assign('list', $list);
			$this->assign('pid', $pid);
            $this->meta_title = '节点排序';
            $this->display();
        }elseif (IS_POST){
            $ids = I('post.ids');
            $ids = explode(',', $ids);
			$pid = I('post.pid');
			$node_model = D("Node");
            foreach ($ids as $key=>$value){
                $res = $node_model->where(array('id'=>$value))->setField('sort', $key+1);
            }
            if($res !== false){
                $this->success('排序成功', U('index',array('pid'=>$pid)));
            }else{
                $this->eorror('排序失败', U('index',array('pid'=>$pid)));
            }
        }else{
            $this->error('非法请求');
        }
	}
}