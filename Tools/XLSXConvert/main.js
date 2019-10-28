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
    regex = /"\t/gi
    csvData = csvData.replace(regex, '\t')
    
    if(!fs.existsSync('./config')) {
        fs.mkdirSync('./config')
    }
    fs.writeFileSync(`./config/${sheetName}.csv`, csvData)
})