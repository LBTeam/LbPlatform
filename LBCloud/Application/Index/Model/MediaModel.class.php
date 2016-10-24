<?php
/**
 * 播放媒体model
 * @author liangjian
 * @email 15934854815@163.com
 */
 
namespace Index\Model;
use Think\Model;

class MediaModel extends Model
{
	/**
	 * 需清理媒体列表
	 * @param $size 媒体大小（超过此值清理）
	 * @param $time 过期时间（小于此值清理）
	 * @return array
	 */
	public function clean_list($size, $time){
		if($size && $time){
			$map = array();
			$map['size'] = array("GT", $size);
			$map['publish'] = array("ELT", $time);
			$map['status'] = 1;
			$list = $this->where($map)->getField("id,object");
			return $list ? $list : array();
		}
		return array();
	}
}
