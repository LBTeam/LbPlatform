<?php
/**
 * 节点model
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Index\Model;
use Think\Model;

class NodeModel extends Model
{
	/**
	 * 获取所有启用节点列表
	 */
	public function get_all_enable_nodes(){
		$nodes = $this->where("status=0")->order('sort')->select();
		return $nodes;
	}
	
	/**
	 * 获取所有节点列表
	 */
	public function get_all_nodes(){
		$nodes = $this->order('sort')->select();
		return $nodes;
	}
	
	/**
	 * 获取所有菜单节点列表
	 */
	public function get_all_menu_nodes(){
		$map = array();
		$map['status'] = 0;
		$map['level'] = array("NEQ", 3);
		$nodes = $this
					->field("id,name,title,pid")
					->where($map)
					->order('sort')
					->select();
		return $nodes;
	}
	
	/*
	 * 获取所有菜单中文名字
	 */
	public function all_node_title(){
		return $this->getField("id,title");
	}
	
	/**
	 * 根据父id获取所有节点列表
	 */
	public function all_node_by_pid($pid=0, $title=''){
		$map = array();
		$map['pid'] = $pid;
		if(!empty($title) === true)
			$map['title'] = array('LIKE', "%{$title}%");
		return $this
				->where($map)
				->field(true)
				->order('sort asc,id asc')
				->select();
	}
	
	/**
	 * 节点详情
	 */
	public function node_by_id($id, $field="*"){
		if($id){
			return $this->field($field)->find($id);
		}
		return array();
	}
}
