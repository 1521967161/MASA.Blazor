pipeline {
    agent {
        label 'k8s-slave-pipeline'
    }
    options {
            timeout(time: 30, unit: 'MINUTES')
            buildDiscarder(logRotator(numToKeepStr: '5', artifactNumToKeepStr: '5'))
    }
    environment {
        IMAGE = """${sh(
                returnStdout: true,
                script: 'echo  ${NEW_ALI_REGISTRY}"/masa/masa-blazor-docs:0.2."${BUILD_ID}'
            )}"""
        IMAGE_PRD =  """${sh(
                returnStdout: true,
                script: 'echo  ${NEW_ALI_REGISTRY}"/masa/masa-blazor-docs:"${GIT_BRANCH}'
            )}"""
        NEW_ALI_REGISTRY_AUTH = credentials('NEW_ALI_REGISTRY_AUTH')
        KUBE_CONFIG_DEV = credentials('k8s-ack')
        KUBE_CONFIG_PRD = credentials('k8s-ack-prd')
        NUGET_KEY = credentials('nuget-key')
    }
    stages {
        stage('setting env') {
            agent any
            options {
              skipDefaultCheckout(true)
            }
            steps {
                wrap([$class: 'BuildUser']) {
                    script {
                        BUILD_USER = "${BUILD_USER}"
                        BUILD_USER_ID ="${BUILD_USER_ID}"
                    }
                    script {
                        env.BUILD_USERNAME = "${BUILD_USER}"
                        env.BUILD_USERNAMEID = "${BUILD_USER_ID}"
                    }
                }
            }
        }
        stage('packer-dev') {
            options {
                retry (2)
            }
            when {
                branch 'develop'
            }
            steps{
                container('dotnet-sdk') {
                    sh '''
                          ls src/
                          node -v
                          dotnet --version
                          git clone -b develop https://github.com/BlazorComponent/BlazorComponent.git ./src/BlazorComponent
                          dotnet build src
                          dotnet pack src --include-symbols -p:PackageVersion=0.2."${BUILD_ID}"
                          dotnet nuget push "**/*.symbols.nupkg" -k $NUGET_KEY -s https://api.nuget.org/v3/index.json
                       '''
                           
                }
            }
        }
        stage('docker-dev') {
            options {
                retry (2)
            }
            when {
                branch 'develop'
            } 
            steps {
                container('docker') {
                    sh '''
                          docker login $NEW_ALI_REGISTRY --username=$NEW_ALI_REGISTRY_AUTH_USR -p $NEW_ALI_REGISTRY_AUTH_PSW
                          docker build -t $IMAGE .
                          docker push $IMAGE
                          docker rmi  $IMAGE
                       '''
                }
            }    
        }
        //发布开发环境
        stage('deploy-dev') {
            when {
                branch 'develop'
            }
            steps {
                container('kubectl') {
                    sh '''
                          echo $KUBE_CONFIG_DEV | base64 --decode >> ./config
                          kubectl --kubeconfig ./config set image deployment/masa-blazor-docs masa-blazor-docs=$IMAGE -n masa-blazor
                       '''
                }
            }
        }
        //发布测试环境
        stage('deploy-test') {
            when {
                branch 'develop'
            }
            steps {
                container('kubectl') {
                    sh '''
                          echo $KUBE_CONFIG_DEV | base64 --decode >> ./config
                          kubectl --kubeconfig ./config set image deployment/masa-blazor-docs masa-blazor-docs=$IMAGE -n masa-blazor
                       '''
                }
            }
        }
        stage('packer-prd') {
            options {
                retry (2)
            }
            when {
                buildingTag()
            }
            steps{
                container('dotnet-sdk') {
                    sh '''
                          ls src/
                          node -v
                          dotnet --version
                          git clone -b develop https://github.com/BlazorComponent/BlazorComponent.git ./src/BlazorComponent
                          dotnet build src
                          dotnet pack src --include-symbols -p:PackageVersion=${GIT_BRANCH}"
                          dotnet nuget push "**/*.symbols.nupkg" -k $NUGET_KEY -s https://api.nuget.org/v3/index.json
                       '''
                           
                }
            }
        }
        stage('docker-prd') {
            options {
                retry (2)
            }
            when {
                buildingTag()
            } 
            steps {
                container('docker') {
                    sh '''
                          docker login $NEW_ALI_REGISTRY --username=$NEW_ALI_REGISTRY_AUTH_USR -p $NEW_ALI_REGISTRY_AUTH_PSW
                          docker build -t $IMAGE_PRD .
                          docker push $IMAGE_PRD
                          docker rmi  $IMAGE_PRD
                       '''
                }
            }    
        }
        stage('deploy-prd') {
            when {
                buildingTag()
            }
            steps {
                container('kubectl') {
                    sh '''
                          echo $KUBE_CONFIG_PRD | base64 --decode >> ./config
                          kubectl --kubeconfig ./config set image deployment/masa-blazor-docs masa-blazor-docs=$IMAGE -n masa-blazor
                       '''
                }
            }
        }
    }
    post {
        success {
            script {
                sh 'export TYPE=success;export JOB_NAME="${JOB_BASE_NAME}";export BUILD_NUM="$BUILD_NUMBER";export BUILD_TIME="$BUILD_TIMESTAMP";export BUILD_USER="${BUILD_USERNAME}"; export URL_JOB="${BUILD_URL}";export URL_LOG="${BUILD_URL}console";export JOB_TIPS1="${BUILD_USERNAMEID}" ;sh send_message-export.sh'
            }
        }
        failure {
            script {
                sh 'export TYPE=failure;export JOB_NAME="${JOB_BASE_NAME}";export BUILD_NUM="$BUILD_NUMBER";export BUILD_TIME="$BUILD_TIMESTAMP"; export BUILD_USER="${BUILD_USERNAME}"; export URL_JOB="${BUILD_URL}";export URL_LOG="${BUILD_URL}console";export JOB_TIPS1="${BUILD_USERNAMEID}" ;sh send_message-export.sh'
            }
        }
    }
}