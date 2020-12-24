﻿import bus from "../../Util/bus.js";
import PaneledBlockTemplate from "../../Templates/PaneledBlockTemplate.js";
import SecondaryBlock from "../../Controls/SecondaryBlock.js";
import RockButton from "../../Elements/RockButton.js";
import TextBox from "../../Elements/TextBox.js";
import { defineComponent } from '../../Vendor/Vue/vue.js';

export default defineComponent({
    name: 'Test.PersonSecondary',
    components: {
        PaneledBlockTemplate,
        SecondaryBlock,
        TextBox,
        RockButton
    },
    data() {
        return {
            messageToPublish: '',
            receivedMessage: ''
        };
    },
    methods: {
        receiveMessage(message) {
            this.receivedMessage = message;
        },
        doPublish() {
            bus.publish('PersonSecondary:Message', this.messageToPublish);
            this.messageToPublish = '';
        }
    },
    computed: {
        currentPerson() {
            return this.$store.state.currentPerson;
        },
        currentPersonName() {
            return this.currentPerson ? this.currentPerson.FullName : 'anonymous';
        },
        imageUrl() {
            if (this.currentPerson && this.currentPerson.PhotoUrl) {
                return this.currentPerson.PhotoUrl;
            }

            return '/Assets/Images/person-no-photo-unknown.svg'
        },
        photoElementStyle() {
            return `background-image: url("${this.imageUrl}"); background-size: cover; background-repeat: no-repeat;`
        }
    },
    created() {
        bus.subscribe('PersonDetail:Message', this.receiveMessage);
    },
    template:
`<SecondaryBlock>
    <PaneledBlockTemplate>
        <template v-slot:title>
            <i class="fa fa-flask"></i>
            Secondary Block
        </template>
        <template v-slot:default>
            <div class="row">
                <div class="col-sm-6">
                    <p>
                        Hi, {{currentPersonName}}!
                        <div class="photo-icon photo-round photo-round-sm" :style="photoElementStyle"></div>
                    </p>
                    <p>This is a secondary block. It respects the store's value indicating if secondary blocks are visible.</p>
                </div>
                <div class="col-sm-6">
                    <div class="well">
                        <TextBox label="Message" v-model="messageToPublish" />
                        <RockButton class="btn-primary btn-sm" @click="doPublish">Publish</RockButton>
                    </div>
                    <p>
                        <strong>Detail block says:</strong>
                        {{receivedMessage}}
                    </p>
                </div>
            </div>
        </template>
    </PaneledBlockTemplate>
</SecondaryBlock>`
})
