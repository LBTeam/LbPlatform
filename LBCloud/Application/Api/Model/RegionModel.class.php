<?php
/**
 * 全国地区model
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Api\Model;
use Think\Model;

class RegionModel extends Model
{
	public function all_region_name(){
		return $this->getField("id,name");
	}
}
