<?php
/**
 * 用户组管理控制器
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Index\Controller;

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
	 * 更改状态
	 */
	public function status($id,$value = 0){
        $id    = is_array($id) ? implode(',',$id) : $id;
        $where = array_merge( array('id' => array('in', $id )) ,array('id'=>$id) );
        $msg   = array_merge( array( 'success'=>'操作成功！', 'error'=>'操作失败！', 'url'=>'' ,'ajax'=>IS_AJAX) , (array)$msg );
		$data  = array('status'=>$value);
		$role_model = D("Role");
        if( $role_model->where($where)->save($data)!==false ) {
            $this->success($msg['success'],$msg['url'],$msg['ajax']);
        }else{
            $this->error($msg['error'],$msg['url'],$msg['ajax']);
        }
	}
}
