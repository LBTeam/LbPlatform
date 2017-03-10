<?php
/**
 * 播放端版本model
 * @author liangjian
 * @email 15934854815@163.com
 */

namespace Index\Model;
use Think\Model;

class VersionModel extends Model
{
    /**
     * 版本列表
     */
    public function all_packages(){
        return $this->select();
    }

    /**
     * 根据ID获取更新包
     * @param $id
     * @param string $field
     * @return array|mixed
     */
    public function package_by_id($id, $field = '*'){
        if($id){
            return $this->field($field)->find($id);
        }
        return array();
    }

    /**
     * 根据ID数组获取更新包
     * @param $id
     * @param string $field
     * @return array
     */
    public function package_by_ids($ids, $field = '*'){
        if($ids){
            $map = array('id' => array('in', $ids));
            return $this->where($map)->field($field)->select();
        }
        return array();
    }

    /**
     * 最新的更新包
     * @return array
     */
    public function last_package(){
        return $this->order("id DESC")->find();
    }
}