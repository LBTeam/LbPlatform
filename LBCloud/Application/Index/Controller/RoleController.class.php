<?php
/**
 * 用户组管理控制器
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Index\Controller;
use Index\Service\TreeService;

class RoleController extends CommonController
{
	/**
	 * 用户组列表
	 */
	public function index(){
		$role_model = D("Role");
		$roles = $role_model->all_role_list();
		int_to_string($roles,array('status'=>array(1=>'关闭',0=>'开启')));
		$this->assign("roles", $roles);
		$this->meta_title = '用户组列表';
        $this->display();
	}
	
	/**
	 * 添加用户组
	 */
	public function add(){
		$role_model = D("Role");
		if(IS_POST){
			$rules = array(
				array('name','require','用户组不能为空！'),
				array('name','','用户组已经存在！',0,'unique',1)
			);
			$data = array();
			$data['name'] = I("post.name", "");
			$data['status'] = I("post.status", 0);
			$data['pid'] = 0;
			$data['remark'] = I("post.remark", "");
			if($role_model->validate($rules)->create($data)){
				$id = $role_model->add();
				if($id){
                    $this->success('新增成功', U('index'));
                } else {
                    $this->error('新增失败');
                }
			}else{
				$this->error($role_model->getError());
			}
		}else{
			$this->meta_title = '新增用户组';
            $this->display();
		}
	}
	
	/**
	 * 修改用户组
	 */
	public function edit($id = 0){
		$role_model = D("Role");
		if(IS_POST){
			$rules = array(
				array('name','require','用户组不能为空！'),
				array('name','','用户组已经存在！',0,'unique',2)
			);
			$data = array();
			$data['id'] = I("post.id", "");
			$data['name'] = I("post.name", "");
			$data['status'] = I("post.status", 0);
			$data['pid'] = 0;
			$data['remark'] = I("post.remark", "");
            if($role_model->validate($rules)->create($data)){
                if($role_model->save()!== false){
                    $this->success('修改成功', U('index'));
                } else {
                    $this->error('修改失败');
                }
            } else {
                $this->error($role_model->getError());
            }
		}else{
			$info = $role_model->role_by_id($id);
			if($info){
				$this->assign('info', $info);
	            $this->meta_title = '编辑用户组';
	            $this->display();
			}else{
				$this->error('获取用户组信息错误');
			}
		}
	}
	
	/**
	 * 删除用户组
	 */
	public function del(){
		$id = I('request.id', 0);
        if ( empty($id) ) {
            $this->error('请选择要操作的数据!');
        }
		$role_model = D('Role');
		$id = array_unique((array)$id);
        $map = array('id' => array('in', $id) );
        if($role_model->where($map)->delete()){
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
        $where = array('id' => array('in', $id ));
        $msg   = array_merge( array( 'success'=>'操作成功！', 'error'=>'操作失败！', 'url'=>'' ,'ajax'=>IS_AJAX) , (array)$msg );
		$data  = array('status'=>$value);
		$role_model = D("Role");
        if( $role_model->where($where)->save($data)!==false ) {
            $this->success($msg['success'],$msg['url'],$msg['ajax']);
        }else{
            $this->error($msg['error'],$msg['url'],$msg['ajax']);
        }
	}
	
	/**
	 * 用户列表
	 */
	public function user($group_id=0){
		$role_model = D("Role");
		$cfg_model = D("Config");
		$roles = $cfg_model->roles();
		$info = $role_model->role_by_id($group_id);
		$users = $role_model->users_by_role($group_id);
		$condition = array(
    		"status" => array(
    			0 => "正常",
    			1 => "禁用"
    		)
    	);
    	int_to_string($users, $condition);
    	if($group_id == $roles["normal"]){
			$user_model = D("User");
			$agents = $user_model->all_agents();
			foreach ($users as $key => &$value) {
				$value['p_email'] = $agents[$value['puid']]['email'];
    			$value['p_phone'] = $agents[$value['puid']]['phone'];
			}
		}
    	$this->assign("info", $info);
    	$this->assign("users", $users);
    	$this->assign("roles", $roles);
    	$this->assign("role_id", $group_id);
    	$this->meta_title = "用户列表";
    	$this->display();
	}
	
	/**
	 * 授权
	 */
	public function access($group_id=0){
		if(IS_POST){
			$role_id = I("post.role_id", 0);
			$access = I("post.access");
			if($role_id){
				if(!is_array($access)){
					$access = explode(',', $access);
				}
				$data = array();
				foreach($access as $val){
					$data[] = array(
						'role_id' => $role_id,
						'node_id' => $val
					);
				}
				$model = new \Think\Model();
				$access_model = D("Access");
				$model->startTrans();
				$del_res = $access_model->del_by_role($role_id);
				$add_res = $access_model->add_multiples($data);
				if($del_res !== false && $add_res !== false){
					$model->commit();
					$this->success('授权成功！', U("access", array("group_id"=>$role_id)));
				}else{
					$model->rollback();
					$this->error('授权失败！');
				}
			}else{
				$this->error('授权失败，系统错误！');
			}
		}else{
			$node_model = D("Node");
			$access_model = D("Access");
			$role_model = D("Role");
			$treeService = new TreeService();
			$nodes = $node_model->get_all_nodes();
			$access = $access_model->access_by_role($group_id);
			foreach($nodes as &$value){
				if(in_array($value['id'], $access)){
					$value['checked'] = "1";
				}else{
					$value['checked'] = "0";
				}
			}
			$nodes = $treeService->create_menu($nodes);
			$info = $role_model->role_by_id($group_id);
			$this->meta_title = "用户组授权";
			$this->assign("nodes", $nodes);
			$this->assign("info", $info);
			$this->display();
		}
	}
}
