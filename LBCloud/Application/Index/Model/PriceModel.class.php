<?php
/**
 * 时段价格model
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Index\Model;
use Think\Model;

class PriceModel extends Model
{
	/*
	 * 屏幕价格列表
	 */
	public function price_by_screen($sid, $id=0){
		if($sid){
			$map = array();
			$map['screen_id'] = $sid;
			if($id){
				$map['id'] = array("NEQ", $id);
			}
			return $this->where($map)->select();
		}
		return false;
	}
	
	/**
	 * 价格详情
	 */
	public function price_by_id($id, $field="*"){
		if($id){
			return $this->field($field)->find($id);
		}
		return array();
	}
}
