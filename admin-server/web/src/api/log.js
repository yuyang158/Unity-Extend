import request from '@/utils/request'

export function getList(params) {
  params = { min: 0, max: 50 }
  return request({
    url: 'file/query/log',
    method: 'get',
    params
  })
}

export function getMaxId() {
  return request({
    url: 'file/max/log',
    method: 'get',
  })
}
