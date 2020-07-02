<template>
  <div class="app-container">
    <el-table
      v-loading="listLoading"
      :data="list"
      element-loading-text="Loading"
      border
      fit
      highlight-current-row
    >
      <el-table-column align="center" label="ID" width="95">
        <template slot-scope="scope">
          {{ scope.row.id }}
        </template>
      </el-table-column>
      <el-table-column label="File Name">
        <template slot-scope="scope">
          {{ scope.row.path }}
        </template>
      </el-table-column>
      <el-table-column align="center" prop="created_at" label="Upload time" width="250">
        <template slot-scope="scope">
          <i class="el-icon-time" />
          <span>{{ scope.row.time }}</span>
        </template>
      </el-table-column>
      <el-table-column align="center" label="Actions" width="120">
        <template slot-scope="{row}">
          <el-button
            type="primary"
            size="small"
            icon="el-icon-circle-check-outline"
            @click="download(row)"
          >
            Download
          </el-button>
        </template>
      </el-table-column>
    </el-table>
  </div>
</template>

<script>
import { getList, getMaxId } from '@/api/log'

export default {
  filters: {
    statusFilter(status) {
      const statusMap = {
        published: 'success',
        draft: 'gray',
        deleted: 'danger'
      }
      return statusMap[status]
    }
  },
  data() {
    return {
      list: null,
      listLoading: true,
      pageIndex: 0
    }
  },
  created() {
    this.fetchData()
  },
  methods: {
    download(row) {
      window.open(`http://127.0.0.1:3000/${row.path}`, '_blank')
    },
    fetchData() {

      this.listLoading = true
      getList().then(response => {
        for (const row of response.results) {
          row.time = new Date(Date.parse(row.time)).toLocaleString()
        }
        this.list = response.results
        this.listLoading = false
      })
    }
  }
}
</script>
