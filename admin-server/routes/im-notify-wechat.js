const bent = require("bent")
const config = require('config')
const notifyBaseUrl = config.get('im.wechat.notifyBaseUrl')
const notifyUrlPath = config.get('im.wechat.notifyUrlPath')

module.exports.notify = async function (downloadLink, viewLink, pageLink, mentionList) {
  const post = bent(notifyBaseUrl, "POST", "json")
  const content = `# Log uploaded.\n ## 下载: [链接](${downloadLink})\n## 在线查看: [链接](${viewLink})`
  console.log(``)
  const response = await post(notifyUrlPath, {
    msgtype: "markdown",
    markdown: { content, mentioned_list: mentionList },
  })
  console.log(`${JSON.stringify(response)}`)
}
