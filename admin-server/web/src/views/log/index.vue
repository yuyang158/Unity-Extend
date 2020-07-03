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
    <pagination v-show="total>0" :total="total" :page.sync="listQuery.page" :limit.sync="listQuery.limit" @pagination="fetchData" />
  </div>
</template>

<script>
import { getList, getMaxId } from '@/api/log'
import Pagination from '@/components/Pagination'

export default {
  components: { Pagination },
  data() {
    return {
      list: null,
      listLoading: true,
      total: 0,
      listQuery: {
        page: 1,
        limit: 20
      }
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
      getMaxId().then(response => {
        const maxCount = response.count
        if (maxCount < (this.listQuery.page - 1) * 50) {
          this.listQuery.page = Math.floor(Math.max(0, maxCount - 1) / 50) + 1
        }
        this.total = maxCount
        getList(this.listQuery.page - 1).then(response => {
          for (const row of response.results) {
            row.time = new Date(Date.parse(row.time)).toLocaleString()
          }
          this.list = response.results
          this.listLoading = false
        })
      })
      setTimeout(() => {
        this.listLoading = false
      }, 5000)
    }
  }
}
</script>
