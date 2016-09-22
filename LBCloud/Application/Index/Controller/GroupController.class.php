<?php
/**
 * 屏幕分组controller
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Index\Controller;

class GroupController extends CommonController
{
	/**
	 * 列表
	 */
	public function index(){
		$group_model = D("Group");
		$groups = $group_model->group_by_uid(ADMIN_UID);
		$this->assign("groups", $groups);
		$this->meta_title = '分组列表';
        $this->display();
	}
	
	/**
	 * 添加
	 */
	public function add(){
		if(IS_POST){
			$name = I("post.name", "");
			$remark = I("post.remark", "");
			if($name){
				$group_model = D("Group");
				if(!$group_model->check_group($name, ADMIN_UID)){
					$data = array();
					$data['uid'] = ADMIN_UID;
					$data['name'] = $name;
					$data['remark'] = $remark;
					$data['addtime'] = NOW_TIME;
					if($group_model->add($data)){
						$this->success("添加成功！", U('index'));
					}else{
						$this->error("添加失败");
					}
				}else{
					$this->error("屏幕组已存在！");
				}
			}else{
				$this->error("屏幕组名称不能为空！");
			}
		}else{
			$this->meta_title = '新增屏幕组';
            $this->display();
		}
	}
	
	/**
	 * 修改
	 */
	public function edit($id = 0){
		$group_model = D("Group");
		if($id && ADMIN_UID == $group_model->group_owner($id)){
			if(IS_POST){
				$name = I("post.name", "");
				$remark = I("post.remark", "");
				if($name){
					if(!$group_model->check_group($name, ADMIN_UID, $id)){
						$data = array();
						$data['id'] = $id;
						$data['name'] = $name;
						$data['remark'] = $remark;
						if(false !== $group_model->save($data)){
							$this->success("修改成功！", U('index'));
						}else{
							$this->error("修改失败");
						}
					}else{
						$this->error("屏幕组已存在！");
					}
				}else{
					$this->error("屏幕组名称不能为空！");
				}
			}else{
				$info = $group_model->group_by_id($id);
				if($info){
					$this->assign("info", $info);
					$this->meta_title = '修改屏幕组';
	           		$this->display();
				}else{
					$this->error('获取屏幕组信息错误');
				}
			}
		}else{
			$this->error('系统错误，非法请求！');
		}
	}
	
	/**
	 * 删除
	 */
	public function del(){
		$id = I('request.id', 0);
        if ( empty($id) ) {
            $this->error('请选择要操作的数据!');
        }
		$group_model = D('Group');
		$id = array_unique((array)$id);
        $map = array('id' => array('in', $id) );
        $map2 = $map;
        $map2['uid'] = array('NEQ', ADMIN_UID);
        $temps = $group_model->where($map2)->count();
        if($temps == 0){
        	$model = new \Think\Model();
        	$model->startTrans();
        	$del = $group_model->where($map)->delete();
        	$unbind = $group_model->unbind_relation($id);
        	if($del && $unbind !== false){
        		$model->commit();
	            $this->success('删除成功');
	        } else {
	        	$model->rollback();
	            $this->error('删除失败！');
	        }
        }else{
        	$this->error('系统错误，删除失败！');
        }
	}
}