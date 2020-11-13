<template>
  <div class="app-container">
    <el-row>
      <el-table
        v-loading="listLoading"
        :data="devices"
        element-loading-text="Loading"
        border
        fit
        highlight-current-row
        @current-change="handleCurrentChange"
      >
        <el-table-column align="center" label="ID" width="95">
          <template slot-scope="scope">
            {{ scope.row.uid }}
          </template>
        </el-table-column>
        <el-table-column label="Device Name">
          <template slot-scope="scope">
            {{ scope.row.name }}
          </template>
        </el-table-column>
      </el-table>
    </el-row>
    <div style="height: 20px" />
    <el-row>
      <el-form ref="luaForm" :model="currentDevice" label-width="80px">
        <el-form-item label="选中设备">
          <el-input v-model="currentDevice.name" :disabled="true" />
        </el-form-item>
        <el-form-item label="Lua脚本">
          <el-input v-model="luaScript" :autosize="{ minRows: 10 }" type="textarea" />
        </el-form-item>
        <el-form-item label="执行结果">
          <el-input v-model="execResult" :autosize="{ minRows: 1 }" type="textarea" />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" :disabled="!currentDevice.uid" @click="onSubmit">发送</el-button>
        </el-form-item>
      </el-form>
    </el-row>
  </div>
</template>

<script>
import { getDevices, doCommand } from '@/api/lua-remote-cmd'
import Cookie from 'js-cookie'

export default {
  name: 'LuaRemoteCmd',
  data() {
    return {
      devices: [],
      listLoading: true,
      luaScript: '',
      execResult: '',
      currentDevice: {
        ret: ''
      }
    }
  },
  watch: {
    luaScript: function(newValue) {
      Cookie.set('lua', newValue)
    }
  },
  created() {
    this.listLoading = true
    this.luaScript = Cookie.get('lua')
    getDevices().then(response => {
      this.listLoading = false
      this.devices = JSON.parse(response.content)
    })
  },
  methods: {
    handleCurrentChange(val) {
      this.currentDevice = val
      console.log(val)
    },
    onSubmit() {
      doCommand(this.currentDevice.uid, this.luaScript).then(response => {
        this.execResult = response.content
        console.log(response)
      })
    }
  }
}
</script>

<style scoped>

</style>
