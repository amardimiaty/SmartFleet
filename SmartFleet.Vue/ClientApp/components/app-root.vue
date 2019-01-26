<template>
  <div id="app" style="height:100%; width:100%">
  
    <el-container>
    <el-header>
       <nav-menu params="route: route">

       </nav-menu>
    </el-header>
  
  </el-container>
  <el-container >
      <el-aside  width="250px">
        <left-panel></left-panel>
    </el-aside>
    <el-main>
           <my-map></my-map>
    </el-main>
  </el-container>
    <router-view></router-view>
  </div>

</template>
<script>
import Vue from 'vue'
  import NavMenu from './nav-menu'
  import MyAwesomeMap from './map'
  import leftPanel from './leftPanel'
import 'element-ui/lib/theme-chalk/index.css';
  import Container from 'element-ui'
  Vue.use(Container);
  export default {
    components: {
      'nav-menu': NavMenu,
      'my-map': MyAwesomeMap,
      'left-panel': leftPanel
      
    },

    data() {
      return {
        height: 600,
        width : 230
      }
    }
  }
</script>
<style>
  .el-header {
   
    line-height: 45px;
  }
  
  .el-aside {
    color: #333;
    height: 600px;
    margin-top: -15px
  }
  .el-main {
    margin-top: -15px
  }
</style>