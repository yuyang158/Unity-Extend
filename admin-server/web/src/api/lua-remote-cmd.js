import request from '@/utils/request'

export function getDevices() {
  return request({
    url: '/lrc/devices',
    method: 'get'
  })
}

export function doCommand(device, content) {
  return request({
    url: '/lrc/cmd',
    method: 'post',
    data: { device: device, lua: content }
  })
}
