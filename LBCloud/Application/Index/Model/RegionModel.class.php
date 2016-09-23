<?php
/**
 * 全国地区model
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Index\Model;
use Think\Model;

class RegionModel extends Model
{
	public function all_region($pid=false){
		if($pid === false){
			$regions = $this->getField("id,name");
			$regions[0] = "";
		}else{
			$pid = $pid ? $pid : 1;
			$regions = $this
						->where("parent_id={$pid}")
						->getField("id,name");
		}
		return $regions;
	}
}
