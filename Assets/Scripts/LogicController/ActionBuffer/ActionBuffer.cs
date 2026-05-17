using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.LogicController.ActionBuffer
{
    class ActionBuffer
    {
        public List<ActionRequest> ActionRequests { get; private set; } = new(5);

        public void AddAction(PlayerActions action, float lifeTime = 0.2f)
        {
            int index = FindActionRequestIndex(action);
            if (index != -1)
            {
                ActionRequests[index] = ActionRequests[index].Overwrite(lifeTime);
            }
            else
            {
                ActionRequest actionRequest = new(action, lifeTime);
                ActionRequests.Add(actionRequest);
            }
        }

        public void Update(float deltaTime)
        {
            if (ActionRequests.Count == 0) return;
            for (int i = ActionRequests.Count - 1; i >= 0; i--)
            {
                if (ActionRequests[i].Action == PlayerActions.None) continue;
                ActionRequest actionRequest = ActionRequests[i].UpdateLifeTime(deltaTime);
                if (actionRequest.IsTimedOut)
                {
                    ActionRequests.RemoveAt(i);
                }
                else
                {
                    ActionRequests[i] = actionRequest;
                }
            }
        }

        public ActionRequest GetOldestActionRequest()
        {
            if (ActionRequests.Count != 0) return ActionRequests[0];
            else
            {
                return new ActionRequest(PlayerActions.None, 0);
            }
        }

        public void RemoveAction(ActionRequest actionRequest)
        {
            int index = FindActionRequestIndex(actionRequest.Action);
            if (index != -1)
            {
                ActionRequests.RemoveAt(index);
            }
        }

        private int FindActionRequestIndex(PlayerActions action)
        {
            int index = -1;
            if (ActionRequests.Count == 0) return index;
            for (int i = 0; i < ActionRequests.Count; i++)
            {
                if (ActionRequests[i].Action == action)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }
    }
}
