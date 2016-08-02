<?php
namespace app\home\controller;

class Index
{
    public function index()
    {
    	dump(url('api/Index/index'));
        echo "hello, home module";
    }
}
