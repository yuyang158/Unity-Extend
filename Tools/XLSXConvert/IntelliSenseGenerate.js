const fs = require('fs')
const path = require('path')

const CONFIG_DIRECTORY = './config/'

function ConvertType(typ, key) {
    switch(typ) {
        case "int":
            return "number"
        case "link":
            return key
        default:
            return typ
    }
}

const confileFiles = fs.readdirSync(CONFIG_DIRECTORY).filter(function(path) {
    if(path.indexOf('.csv'))
        return true
    return false
})

let intelliSense = ''
confileFiles.forEach((filename) => {
    const csvFileContent = fs.readFileSync(CONFIG_DIRECTORY + filename, 'utf8')
    const lines = csvFileContent.split('\n')
    const keys = lines[0].split('\t')
    const types = lines[1].split('\t')
    const descriptions = lines[2].split('\t')
    const sheetName = path.basename(filename, '.csv')
    let emmyLua = `---@class ${sheetName}\r\n`
    for(let i = 0; i < keys.length; i++) {
        const key = keys[i]
        const typ = ConvertType(types[i], key)
        emmyLua += `---@field ${key} ${typ} @${descriptions[i]}\r\n`
    }

    intelliSense += emmyLua + '\r\n'
})

fs.writeFileSync(CONFIG_DIRECTORY + 'IntelliSense.lua', intelliSense)