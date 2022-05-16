import request from '@/utils/request'

export function getList(pageIndex) {
  return request({
    url: '/file/query/log?whoami=bing',
    method: 'get',
    params: { index: pageIndex }
  })
}

export function getMaxId() {
  return request({
    url: '/file/count/log?whoami=bing',
    method: 'get'
  })
}
