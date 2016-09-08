<?php
namespace Index\Controller;
use Think\Controller;

class DemoController extends Controller
{
	public function index(){
		echo random_string(32);
	}
}
