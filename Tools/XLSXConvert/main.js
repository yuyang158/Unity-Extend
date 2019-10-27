const XLSX = require('xlsx')
const fs = require('fs')

const wb = XLSX.readFile( './test_excel.xlsx' )
wb.SheetNames.forEach(sheetName => {
    const sheet = wb.Sheets[sheetName]
    let csvData = XLSX.utils.sheet_to_csv(sheet, {FS: '\t', blankrows: false})
    let regex = /""/gi
    csvData = csvData.replace(regex, '"')
    regex = /\t"/gi
    csvData = csvData.replace(regex, '\t')
    
    fs.writeFileSync(`./${sheetName}.csv`, csvData)

    const lines = csvData.split('\n')
    const keys = lines[0].split('\t')
    const types = lines[1].split('\t')

    let emmyLua = `---@class ${sheetName}\n`
    for(let i = 0; i < keys.length; i++) {
        const key = keys[i]
        
        emmyLua += '---@field '
    }
})