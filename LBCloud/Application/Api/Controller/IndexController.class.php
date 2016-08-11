<?php
namespace Api\Controller;
use Think\Controller;
use Api\Service\AliyunOSS;

class IndexController extends Controller {
    public function index()
    {
    	//$str = "OxWaEspL1Dw+ii1Ou+maPqWtciG6+xVAsAJK2ALk/dNVSaUmu6FbTQqwfbQv3x2BfReUAEniEDfit6uvzUmk8PJcjhqGF6Nqok93buh2xCF54uFCVe7xCiIxiUbZWYbDuAf/2Mz5otxU93icmzfBPSAJrS7H6rIO+E+hXTQcvoJ+Mc/9gDVAOhWGquN5B6XrwBRhsEGWEYUVP0o6hkEB0N79iPWB9982+g9OqGzHKS1fWjspA7+Bc4KoSXXncpUBbmvhfCaodbi/nShNQOVp/3AOOE7hEtnIYZW2lDRF2nYtTiCLLUeXlyBj+uL8WHjMpE4xt7O3RCdHg8/0fOgrUQ==";
    	//dump(decrypt($str));
		//exit;
		
    	//echo random_string(31);
		//exit;
		
		
    	//$screen_model = D("Screen");
		//$result = $screen_model->user_all_screen("1");
		//dump($result);
		//exit;
		
    	$AliyunOSS = new AliyunOSS();
		$bucket = C("aliyun_oss_bucket");
		$object = "201608021433/wx_notify.php";
		
		//$result = $AliyunOSS->generate_upload_part(2014568, 102400);
		//dump($result);
		
		//$result = $AliyunOSS->object_list($bucket, "201608021433/");
		//dump($result);
		
		//$AliyunOSS->complete_upload("57a31bb736fda.txt", "F77135AFC16A4F538211515502EC1CFD", array());
		
		//$result = $AliyunOSS->upload_part_list();
		//dump($result);
		
		//$result = $AliyunOSS->part_list("57a31bcc1846c.txt", "A61D6A9B644B41B7A20A3CAD2C49BAC7");
		//dump($result);
		
		//$result = $AliyunOSS->get_upload_id(".txt");
		//dump($result);

		//$result = $AliyunOSS->upload_part_sign($result['Key'], $result['UploadId']);
		//dump($result);
		
		//$buckets = $AliyunOSS->bucket_list();
		//dump($buckets);
		
		//$result = $AliyunOSS->download_uri($bucket, $object);
		//dump($result);
		
		//$result = $AliyunOSS->object_meta($bucket, $object);
		//dump($result);
    }
}