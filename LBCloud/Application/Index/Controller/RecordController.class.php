<?php
/**
 * 播放记录控制器
 * @author liangjian
 * @email 15934854815@163.com
 */
namespace Index\Controller;

class RecordController extends CommonController
{
	public function index(){
		$media = I("media", "");
		$name = I("name", "");
		$rc_model = D("Record");
		$list = $rc_model->record_by_uid(ADMIN_UID, $media, $name);
		$this->meta_title = "播放记录";
		$this->assign("list", $list);
		$this->assign("media", $media);
		$this->assign("name", $name);
		$this->display();
	}
	
	public function export(){
		$media = I("media", "");
		$name = I("name", "");
		$rc_model = D("Record");
		$list = $rc_model->record_by_uid(ADMIN_UID, $media, $name);
		$datas = array();
		foreach($list as $val){
			$temp = $val['email'] ? $val['email'] : $val['phone'];
			$datas[] = array(
				$val['id'],
				$val['media_name'],
				$val['s_name'],
				$temp,
				$val['start'],
				$val['end'],
				date("Y-m-d H:i:s", $val['addtime'])
			);
		}
		$titles = array(
			"ID",
			"节目名称",
			"屏幕名称",
			"所有者",
			"播放开始时间",
			"播放结束时间",
			"记录上传时间"
		);
		$filename = "Lbcloud媒体播放记录";
		$this->_export_to_excel($datas, $titles, $filename);
	}
	
	
	/**
	 * 导出数组到EXCEL文件
	 */
	private function _export_to_excel($datas, $titles, $filename){
		Vendor("PhpExcel.PHPExcel");
		$objPHPExcel = new \PHPExcel();
		/* 设置输出的excel文件为2007兼容格式 */
		$objWriter = new \PHPExcel_Writer_Excel2007($objPHPExcel);
		/* 设置当前的sheet */
		$objPHPExcel->setActiveSheetIndex(0);
		$objActSheet = $objPHPExcel->getActiveSheet();
		/*设置宽度*/
		$tNum = 'A';
		foreach($titles as $val){
			$objPHPExcel->getActiveSheet()->getColumnDimension($tNum)->setWidth(20);
			$tNum++;
		}
		/* sheet标题 */
		$objActSheet->setTitle();
		$tNum = 'A';
		foreach($titles as $val){
			$objActSheet->setCellValue($tNum.'1', $val);
			$tNum++;
		}
		$i = 2;
		foreach($datas as $value)
		{
			/* excel文件内容 */
			$j = 'A';
			foreach($value as $value2)
			{
				$objActSheet->setCellValue($j.$i, $value2);
				$j++;
			}
			$i++;
		}
		/* 生成到浏览器，提供下载 */
		ob_end_clean();  //清空缓存
		header("Pragma: public");
		header("Expires: 0");
		header("Cache-Control:must-revalidate,post-check=0,pre-check=0");
		header("Content-Type:application/force-download");
		header("Content-Type:application/vnd.ms-execl");
		header("Content-Type:application/octet-stream");
		header("Content-Type:application/download");
		header('Content-Disposition:attachment;filename="'.$filename.'.xlsx"');
		header("Content-Transfer-Encoding:binary");
		$objWriter->save('php://output');
	}
}
